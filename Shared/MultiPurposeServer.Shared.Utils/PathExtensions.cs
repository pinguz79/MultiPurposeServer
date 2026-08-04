namespace MultiPurposeServer.Shared.Utils
{
    public static class PathExtensions
    {
        public static string NormalizedPath(this string path) => path.Trim().Replace('\\', '/').Trim('/');
        public static string NormalizedPathForComparison(this string path) => path.NormalizedPath().ToUpperInvariant();
    }
}