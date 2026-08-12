using ImageMagick;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

using Portfolio.Api.Application.Models;
using Portfolio.Api.Application.Options;

namespace Portfolio.Api.Application.Services
{
    public sealed class OnnxFaceCropFocusDetector : ICropFocusDetector, IDisposable
    {
        private const int InputSize = 640;
        private static readonly int[] Strides = [8, 16, 32];
        private readonly InferenceSession? _session;
        private readonly float _confidence;
        private readonly ILogger<OnnxFaceCropFocusDetector> _logger;

        public OnnxFaceCropFocusDetector(IOptions<PortfolioMediaOptions> options, ILogger<OnnxFaceCropFocusDetector> logger)
        {
            _logger = logger;
            _confidence = options.Value.FaceDetectionConfidence;

            if (!options.Value.SmartCropEnabled)
            {
                return;
            }

            var modelPath = Path.GetFullPath(Path.Combine(options.Value.RootPath, options.Value.FaceDetectionModelPath));

            try
            {
                _session = new InferenceSession(modelPath);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Impossibile inizializzare il modello ONNX {ModelPath}; viene applicato il crop di fallback.", modelPath);
            }
        }

        public CropFocus? Detect(string sourcePath)
        {
            if (_session is null)
            {
                return null;
            }

            try
            {
                using var image = new MagickImage(sourcePath);
                image.AutoOrient();
                var faces = DetectInImage(image);

                if (faces.Count == 0)
                {
                    faces = DetectInOverlappingTiles(image);
                }

                if (faces.Count == 0)
                {
                    return null;
                }

                var left = faces.Min(face => face.Bounds.X);
                var top = faces.Min(face => face.Bounds.Y);
                var right = faces.Max(face => face.Bounds.X + face.Bounds.Width);
                var bottom = faces.Max(face => face.Bounds.Y + face.Bounds.Height);
                return new CropFocus(left, top, right - left, bottom - top);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Rilevamento volti non riuscito per {SourcePath}; viene applicato il crop di fallback.", sourcePath);
                return null;
            }
        }

        private List<FaceDetection> DetectInImage(MagickImage source)
        {
            using var image = new MagickImage(source);
            var originalWidth = image.Width;
            var originalHeight = image.Height;
            image.Resize((uint)InputSize, (uint)InputSize);
            var resizedWidth = image.Width;
            var resizedHeight = image.Height;
            var offsetX = (InputSize - resizedWidth) / 2d;
            var offsetY = (InputSize - resizedHeight) / 2d;
            image.Extent((uint)InputSize, (uint)InputSize, Gravity.Center, MagickColors.Black);

            var pixels = image.ToByteArray(MagickFormat.Rgb);
            var tensor = new DenseTensor<float>([1, 3, InputSize, InputSize]);

            for (var y = 0; y < InputSize; y++)
            {
                for (var x = 0; x < InputSize; x++)
                {
                    var pixel = (y * InputSize + x) * 3;
                    tensor[0, 0, y, x] = pixels[pixel + 2];
                    tensor[0, 1, y, x] = pixels[pixel + 1];
                    tensor[0, 2, y, x] = pixels[pixel];
                }
            }

            var inputName = _session!.InputMetadata.Keys.Single();
            using var results = _session.Run([NamedOnnxValue.CreateFromTensor(inputName, tensor)]);
            var outputs = results.ToDictionary(result => result.Name, result => result.AsTensor<float>());
            return DecodeFaces(outputs)
                .Select(face => face with { Bounds = MapToOriginal(face.Bounds, originalWidth, originalHeight, resizedWidth, resizedHeight, offsetX, offsetY) })
                .Where(face => face.Bounds.Width > 0 && face.Bounds.Height > 0)
                .ToList();
        }

        private List<FaceDetection> DetectInOverlappingTiles(MagickImage image)
        {
            const double tileRatio = 0.6;
            var tileWidth = (uint)Math.Ceiling(image.Width * tileRatio);
            var tileHeight = (uint)Math.Ceiling(image.Height * tileRatio);
            var centeredX = (image.Width - tileWidth) / 2;
            var centeredY = (image.Height - tileHeight) / 2;
            var origins = new[]
            {
                (X: centeredX, Y: 0u),
                (X: centeredX, Y: centeredY),
                (X: 0u, Y: 0u),
                (X: image.Width - tileWidth, Y: 0u)
            };
            var faces = new List<FaceDetection>();

            foreach (var origin in origins.Distinct())
            {
                using var tile = new MagickImage(image);
                tile.Crop(new MagickGeometry((int)origin.X, (int)origin.Y, tileWidth, tileHeight));

                faces.AddRange(DetectInImage(tile).Select(face => face with
                {
                    Bounds = new CropFocus(
                        (origin.X + face.Bounds.X * tileWidth) / image.Width,
                        (origin.Y + face.Bounds.Y * tileHeight) / image.Height,
                        face.Bounds.Width * tileWidth / image.Width,
                        face.Bounds.Height * tileHeight / image.Height)
                }));
            }

            return ApplyNonMaximumSuppression(faces);
        }

        private List<FaceDetection> DecodeFaces(IReadOnlyDictionary<string, Tensor<float>> outputs)
        {
            var faces = new List<FaceDetection>();

            foreach (var stride in Strides)
            {
                var scores = outputs[$"cls_{stride}"].ToArray();
                var objectness = outputs[$"obj_{stride}"].ToArray();
                var boxes = outputs[$"bbox_{stride}"].ToArray();
                var columns = InputSize / stride;

                for (var index = 0; index < scores.Length; index++)
                {
                    var score = Math.Sqrt(Math.Clamp(scores[index], 0, 1) * Math.Clamp(objectness[index], 0, 1));

                    if (score < _confidence)
                    {
                        continue;
                    }

                    var row = index / columns;
                    var column = index % columns;
                    var centerX = (column + boxes[index * 4]) * stride;
                    var centerY = (row + boxes[index * 4 + 1]) * stride;
                    var faceWidth = Math.Exp(boxes[index * 4 + 2]) * stride;
                    var faceHeight = Math.Exp(boxes[index * 4 + 3]) * stride;
                    var x = Math.Clamp((centerX - faceWidth / 2) / InputSize, 0, 1);
                    var y = Math.Clamp((centerY - faceHeight / 2) / InputSize, 0, 1);
                    var width = Math.Clamp(faceWidth / InputSize, 0, 1 - x);
                    var height = Math.Clamp(faceHeight / InputSize, 0, 1 - y);
                    faces.Add(new FaceDetection(new CropFocus(x, y, width, height), score));
                }
            }

            return ApplyNonMaximumSuppression(faces);
        }

        private static CropFocus MapToOriginal(CropFocus face, uint originalWidth, uint originalHeight, uint resizedWidth, uint resizedHeight, double offsetX, double offsetY)
        {
            var scaleX = (double)resizedWidth / originalWidth;
            var scaleY = (double)resizedHeight / originalHeight;
            var left = Math.Clamp((face.X * InputSize - offsetX) / scaleX / originalWidth, 0, 1);
            var top = Math.Clamp((face.Y * InputSize - offsetY) / scaleY / originalHeight, 0, 1);
            var right = Math.Clamp(((face.X + face.Width) * InputSize - offsetX) / scaleX / originalWidth, 0, 1);
            var bottom = Math.Clamp(((face.Y + face.Height) * InputSize - offsetY) / scaleY / originalHeight, 0, 1);
            return new CropFocus(left, top, right - left, bottom - top);
        }

        private static List<FaceDetection> ApplyNonMaximumSuppression(List<FaceDetection> faces)
        {
            var selected = new List<FaceDetection>();

            foreach (var candidate in faces.OrderByDescending(face => face.Score))
            {
                if (selected.All(face => IntersectionOverUnion(face.Bounds, candidate.Bounds) < 0.3))
                {
                    selected.Add(candidate);
                }
            }

            return selected;
        }

        private static double IntersectionOverUnion(CropFocus first, CropFocus second)
        {
            var intersectionWidth = Math.Max(0, Math.Min(first.X + first.Width, second.X + second.Width) - Math.Max(first.X, second.X));
            var intersectionHeight = Math.Max(0, Math.Min(first.Y + first.Height, second.Y + second.Height) - Math.Max(first.Y, second.Y));
            var intersection = intersectionWidth * intersectionHeight;
            var union = first.Width * first.Height + second.Width * second.Height - intersection;
            return union <= 0 ? 0 : intersection / union;
        }

        public void Dispose() => _session?.Dispose();

        private sealed record FaceDetection(CropFocus Bounds, double Score);
    }
}
