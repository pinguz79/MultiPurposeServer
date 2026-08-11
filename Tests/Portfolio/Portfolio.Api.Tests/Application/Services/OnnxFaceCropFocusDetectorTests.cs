using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Portfolio.Api.Application.Options;
using Portfolio.Api.Application.Services;
using Portfolio.Api.Tests.Infrastructure;

namespace Portfolio.Api.Tests.Application.Services
{
    public class OnnxFaceCropFocusDetectorTests
    {
        [Fact]
        public void Constructor_WhenModelIsInvalid_DoesNotPreventFallback()
        {
            // Arrange
            using var temporaryDirectory = new TemporaryDirectory();
            var modelPath = temporaryDirectory.Combine("invalid.onnx");
            File.WriteAllText(modelPath, "invalid model");
            var options = Options.Create(new PortfolioMediaOptions
            {
                RootPath = temporaryDirectory.Path,
                FaceDetectionModelPath = "invalid.onnx"
            });

            // Act
            using var detector = new OnnxFaceCropFocusDetector(options, NullLogger<OnnxFaceCropFocusDetector>.Instance);
            var result = detector.Detect(temporaryDirectory.Combine("unused.jpg"));

            // Assert
            Assert.Null(result);
        }
    }
}
