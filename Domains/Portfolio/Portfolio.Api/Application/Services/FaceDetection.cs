using Portfolio.Api.Application.Models;

namespace Portfolio.Api.Application.Services
{
    internal sealed record FaceDetection(CropFocus Bounds, double Score);
}
