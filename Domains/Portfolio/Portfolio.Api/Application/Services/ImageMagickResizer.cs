using ImageMagick;

using Portfolio.Api.Application.Models;

namespace Portfolio.Api.Application.Services
{
    public class ImageMagickResizer(ICropFocusDetector cropFocusDetector) : IImageResizer
    {
        private const uint JpegQuality = 80;

        #region Ridimensionamento

        public async Task Resize(string sourcePath, string destinationPath, int width, int height, bool crop)
        {
            var destinationDirectory = Path.GetDirectoryName(destinationPath);

            if (string.IsNullOrWhiteSpace(destinationDirectory))
            {
                throw new InvalidOperationException($"Impossibile determinare la directory della cache dal percorso '{destinationPath}'.");
            }

            Directory.CreateDirectory(destinationDirectory);

            if (Directory.Exists(destinationPath))
            {
                throw new InvalidOperationException($"Il percorso destinato al file immagine è occupato da una directory: '{destinationPath}'.");
            }

            using var image = new MagickImage(sourcePath);

            image.AutoOrient();

            if (crop)
            {
                ResizeAndCrop(image, width, height, cropFocusDetector.Detect(sourcePath));
            }
            else
            {
                ResizeToFit(image, width, height);
            }

            image.Strip();
            image.Format = MagickFormat.Jpeg;
            image.Quality = JpegQuality;

            await image.WriteAsync(destinationPath);
        }

        private static void ResizeToFit(MagickImage image, int width, int height)
        {
            var geometry = new MagickGeometry((uint)width, (uint)height)
            {
                IgnoreAspectRatio = false,
                Greater = true
            };

            image.Resize(geometry);
        }

        private static void ResizeAndCrop(MagickImage image, int width, int height, CropFocus? focus)
        {
            if (focus is not null && !IsFocusSafeInFallback(image, width, height, focus.Value))
            {
                CropAroundFocus(image, width, height, focus.Value);
                return;
            }

            var gravity = image.Height > image.Width
                ? Gravity.North
                : Gravity.Center;

            var geometry = new MagickGeometry((uint)width, (uint)height)
            {
                FillArea = true
            };

            image.Resize(geometry);
            image.Extent((uint)width, (uint)height, gravity);
        }

        #endregion

        #region Calcolo crop

        private static bool IsFocusSafeInFallback(MagickImage image, int width, int height, CropFocus focus)
        {
            var targetRatio = (double)width / height;
            var sourceRatio = (double)image.Width / image.Height;
            double left;
            double top;
            double right;
            double bottom;

            if (sourceRatio > targetRatio)
            {
                var visibleWidth = targetRatio / sourceRatio;
                left = (1 - visibleWidth) / 2;
                right = left + visibleWidth;
                top = 0;
                bottom = 1;
            }
            else
            {
                var visibleHeight = sourceRatio / targetRatio;
                left = 0;
                right = 1;
                top = image.Height > image.Width ? 0 : (1 - visibleHeight) / 2;
                bottom = top + visibleHeight;
            }

            var horizontalContext = Math.Max(0.03, focus.Width * 0.5);
            var contextAbove = Math.Max(0.03, focus.Height * 0.75);
            var contextBelow = Math.Max(0.03, focus.Height);

            return focus.X - horizontalContext >= left
                && focus.Y - contextAbove >= top
                && focus.X + focus.Width + horizontalContext <= right
                && focus.Y + focus.Height + contextBelow <= bottom;
        }

        private static void CropAroundFocus(MagickImage image, int width, int height, CropFocus focus)
        {
            var targetRatio = (double)width / height;
            var focusCenterX = (focus.X + focus.Width / 2) * image.Width;
            var focusCenterY = (focus.Y + focus.Height / 2) * image.Height;
            var sourceRatio = (double)image.Width / image.Height;
            var cropWidth = sourceRatio > targetRatio
                ? image.Height * targetRatio
                : image.Width;
            var cropHeight = sourceRatio > targetRatio
                ? image.Height
                : image.Width / targetRatio;

            var x = Math.Clamp(focusCenterX - cropWidth / 2, 0, image.Width - cropWidth);
            var preferredCenterY = focusCenterY + cropHeight * 0.12;
            var y = Math.Clamp(preferredCenterY - cropHeight / 2, 0, image.Height - cropHeight);
            image.Crop(new MagickGeometry((int)Math.Round(x), (int)Math.Round(y), (uint)Math.Round(cropWidth), (uint)Math.Round(cropHeight)));
            image.Resize((uint)width, (uint)height);
        }
        #endregion

    }
}
