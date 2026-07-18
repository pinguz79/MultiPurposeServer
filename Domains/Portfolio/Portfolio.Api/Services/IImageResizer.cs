namespace Portfolio.Api.Services
{
    public interface IImageResizer
    {
        Task Resize(string sourcePath, string destinationPath, int width, int height, bool crop);
    }
}