using System.IO;

namespace CrossStitch.Core.Utility
{
    public static class FileSystem
    {
        public static string FixPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;
            if (Path.DirectorySeparatorChar != '\\')
                path = path.Replace('\\', Path.DirectorySeparatorChar);
            if (Path.DirectorySeparatorChar != '/')
                path = path.Replace('/', Path.DirectorySeparatorChar);
            return path;
        }

        public static string Combine(params string[] parts)
        {
            var path = Path.Combine(parts);
            return FixPath(path);
        }
    }
}