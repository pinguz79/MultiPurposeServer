using ImageMagick;
using Portfolio.Api.Application.Services;

namespace Portfolio.Api.Services
{
    public class ImageMagickResizer : IImageResizer
    {
        private const uint JpegQuality = 80;

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
                ResizeAndCrop(image, width, height);
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

        private static void ResizeAndCrop(MagickImage image, int width, int height)
        {
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
    }
}
