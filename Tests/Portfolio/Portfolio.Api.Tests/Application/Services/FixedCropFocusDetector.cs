using Portfolio.Api.Application.Models;
using Portfolio.Api.Application.Services;

namespace Portfolio.Api.Tests.Application.Services
{
    internal sealed class FixedCropFocusDetector(CropFocus focus) : ICropFocusDetector
    {
        public CropFocus? Detect(string sourcePath) => focus;
    }
}
