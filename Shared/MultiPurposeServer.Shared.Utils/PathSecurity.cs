namespace MultiPurposeServer.Shared.Utils
{
    public static class PathSecurity
    {
        public static string ResolveRootPath(string contentRootPath, string configuredPath)
        {
            if (string.IsNullOrWhiteSpace(configuredPath))
            {
                throw new InvalidOperationException("Il percorso configurato non può essere vuoto.");
            }

            return Path.IsPathRooted(configuredPath) ? Path.GetFullPath(configuredPath) : Path.GetFullPath(Path.Combine(contentRootPath, configuredPath));
        }

        public static string GetSafePath(string root, string relativePath)
        {
            if (string.IsNullOrWhiteSpace(root))
            {
                throw new ArgumentException("Il percorso root non può essere vuoto.", nameof(root));
            }

            if (string.IsNullOrWhiteSpace(relativePath))
            {
                throw new ArgumentException("Il percorso relativo non può essere vuoto.", nameof(relativePath));
            }

            var fullRoot = Path.GetFullPath(root);
            var fullPath = Path.GetFullPath(Path.Combine(fullRoot, relativePath));
            var rootWithSeparator = Path.EndsInDirectorySeparator(fullRoot) ? fullRoot : fullRoot + Path.DirectorySeparatorChar;

            if (!fullPath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Invalid media path.");
            }

            return fullPath;
        }
    }
}