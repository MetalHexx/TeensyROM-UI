using System.Text.RegularExpressions;

namespace TeensyRom.Core.Common
{
    public static class PathExtensions
    {
        public static string EnsureUnixPathEnding(this string path) => path.EndsWith("/") ? path : path + "/";
        public static string ToUnixPath(this string path) => path.Replace("\\", "/");
        public static string UnixPathCombine(this string path, params string[] paths)
        {
            if (!path.EndsWith("/"))
            {
                path += "/";
            }
            string combined = path + string.Join("/", paths);
            return combined.Replace("//", "/");
        }

        public static string GetUnixParentPath(this string path)
        {
            if (string.IsNullOrEmpty(path) || path == "/") return "/";

            if (!path.StartsWith("/")) path = "/" + path;

            if (path.EndsWith("/")) path = path.TrimEnd('/');

            var segments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length <= 1) return "/";

            var parentPath = string.Join("/", segments.Take(segments.Length - 1));

            return ("/" + parentPath).EnsureUnixPathEnding();
        }

        public static string GetUnixFileExtension(this string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            int lastDotIndex = path.LastIndexOf('.');

            if (lastDotIndex <= 0)
                return string.Empty;

            return path.Substring(lastDotIndex).ToLower();
        }

        public static string RemoveLeadingAndTrailingSlash(this string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }
            if (path.StartsWith("/"))
            {
                path = path[1..];
            }
            if (path.EndsWith("/"))
            {
                path = path[..^1];
            }
            return path;
        }

        public static string GetFileNameFromPath(this string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;

            var segments = path.Split('/');
            return segments.Length > 0 ? segments[^1] : string.Empty;
        }

        public static string GetLastDirectoryFromPath(this string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;

            var segments = path.Split('/');
            return segments.Length > 0 ? segments[^1] : string.Empty;
        }

        public static string ReplaceExtension(this string path, string newExtension)
        {
            if (newExtension.StartsWith(".")) newExtension = newExtension[1..];

            if (string.IsNullOrEmpty(path)) return string.Empty;

            var segments = path.Split('.');
            if (segments.Length > 0)
            {
                segments[^1] = newExtension;
            }
            return string.Join('.', segments);
        }

        public static string[] ToPathArray(this string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return [];
            }
            return path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string GetCommonBasePath(this IEnumerable<string> directories)
        {
            if (!directories.Any()) return string.Empty;

            string commonPath = directories.First();

            foreach (string path in directories)
            {
                while (!path.StartsWith(commonPath, StringComparison.OrdinalIgnoreCase))
                {
                    commonPath = commonPath.Substring(0, commonPath.LastIndexOf(Path.DirectorySeparatorChar));
                }
            }
            return commonPath;
        }

        public static List<string> GetRemainingPathSegments(this string fullPath, string basePath)
        {
            var fullPathItems = fullPath.ToPathArray().ToList();
            var basePathItems = basePath.ToPathArray().ToList();

            fullPathItems = fullPathItems.Select(x => x.ToLower()).ToList();
            basePathItems = basePathItems.Select(x => x.ToLower()).ToList();

            int startIndex = FindStartIndexOfBasePath(fullPathItems, basePathItems);
            if (startIndex == -1)
            {
                return [];
            }
            int remainingPathLength = fullPathItems.Count - (startIndex + basePathItems.Count) - 1;

            if (remainingPathLength <= 0)
            {
                return [];
            }
            List<string> remainingPathSegments = fullPathItems.GetRange(startIndex + basePathItems.Count, remainingPathLength);

            return remainingPathSegments;
        }

        private static int FindStartIndexOfBasePath(List<string> fullPath, List<string> basePath)
        {
            for (int i = 0; i <= fullPath.Count - basePath.Count; i++)
            {
                var currentSlice = fullPath.GetRange(i, basePath.Count);

                if (Enumerable.SequenceEqual(currentSlice, basePath))
                {
                    return i;
                }
            }
            return -1;
        }

        public static bool IsSafeUnixDirectoryName(this string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;

            string forbiddenPattern = @"[\0:*?""<>|;&$`'\\]";

            if (Regex.IsMatch(name, forbiddenPattern) || name.StartsWith("-"))
                return false;

            if (name == "." || name == "..")
                return false;

            return true;
        }
    }
}