using Portfolio.Api.Application.Models;
using Portfolio.Api.Application.Services;

namespace Portfolio.Api.Tests.Application.Services
{
    internal sealed class NoCropFocusDetector : ICropFocusDetector
    {
        public CropFocus? Detect(string sourcePath) => null;
    }
}
