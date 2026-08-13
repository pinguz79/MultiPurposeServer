using SystemPath = System.IO.Path;

namespace Portfolio.Api.Tests.Infrastructure
{
    public sealed class TemporaryDirectory : IDisposable
    {
        public string Path { get; }

        public TemporaryDirectory()
        {
            Path = SystemPath.Combine(SystemPath.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Combine(params string[] paths) => SystemPath.Combine([Path, .. paths]);

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, true);
            }

            GC.SuppressFinalize(this);
        }
    }
}
