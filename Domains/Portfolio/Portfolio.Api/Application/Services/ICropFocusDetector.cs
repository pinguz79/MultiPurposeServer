using Portfolio.Api.Application.Models;

namespace Portfolio.Api.Application.Services
{
    public interface ICropFocusDetector
    {
        CropFocus? Detect(string sourcePath);
    }
}
