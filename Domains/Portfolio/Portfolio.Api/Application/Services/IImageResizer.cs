namespace Portfolio.Api.Application.Services
{
    public interface IImageResizer
    {
        Task Resize(string sourcePath, string destinationPath, int width, int height, bool crop);
    }
}
