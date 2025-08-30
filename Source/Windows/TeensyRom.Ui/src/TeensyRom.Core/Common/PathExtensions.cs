using System.Globalization;
using System.Text.RegularExpressions;

namespace TeensyRom.Core.Common
{
    public static class PathExtensions
    {
        // === Core Path Utilities ===

        public static string EnsureUnixPathEnding(this string path)
        {
            return path.EndsWith("/") ? path : path + "/";
        }

        public static string EnsureUnixPathStart(this string path)
        {
            return path.StartsWith("/") ? path : "/" + path;
        }

        public static string ToUnixPath(this string path)
        {
            return path.Replace("\\", "/");
        }

        public static string UnixPathCombine(this string basePath, params string[] paths)
        {
            var allSegments = new List<string>();

            if (!string.IsNullOrWhiteSpace(basePath))
            {
                allSegments.Add(basePath.Trim('/'));
            }

            foreach (var segment in paths)
            {
                if (!string.IsNullOrWhiteSpace(segment))
                {
                    allSegments.Add(segment.Trim('/'));
                }
            }

            var result = "/" + string.Join("/", allSegments);
            return result.EndsWith("/") ? result : result + "/";
        }

        public static string UnixPathCombineFilePath(this string filePath, params string[] directoryPaths)
        {
            var combinedPaths = "".UnixPathCombine(directoryPaths);
            return (combinedPaths + filePath).Replace("//", "/");
        }

        public static string GetUnixParentPath(this string path)
        {
            if (string.IsNullOrEmpty(path) || path == "/")
            {
                return "/";
            }

            path = path.TrimEnd('/');
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length <= 1)
            {
                return "/";
            }

            return "/" + string.Join("/", segments[..^1]) + "/";
        }

        public static string? GetParentPath(this string path)
        {
            if (string.IsNullOrWhiteSpace(path) || path.Equals("\\") || path.Equals("/"))
            {
                return null;
            }

            char sep = path.Contains('\\') ? '\\' : '/';
            path = path.Replace('\\', sep).Replace('/', sep);

            if (path.Length > 1 && path.EndsWith(sep))
            {
                path = path.TrimEnd(sep);
            }

            if (path == sep.ToString() || Path.GetPathRoot(path)?.TrimEnd(sep) == path)
            {
                return sep.ToString();
            }

            string? parent = Path.GetDirectoryName(path)?.Replace(Path.DirectorySeparatorChar, sep);

            if (string.IsNullOrEmpty(parent))
            {
                return sep.ToString();
            }

            return parent;
        }

        public static string GetUnixFileExtension(this string path)
        {
            return GetFileExtension(path);
        }

        public static string GetFileExtension(this string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            int lastDotIndex = path.LastIndexOf('.');

            if (lastDotIndex <= 0)
            {
                return string.Empty;
            }

            return path[lastDotIndex..].ToLower();
        }

        public static string RemoveLeadingSlash(this string path)
        {
            return string.IsNullOrEmpty(path) ? path : path.TrimStart('/', '\\');
        }

        public static string RemoveLeadingAndTrailingSlash(this string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            if (path is "/" or "\\")
            {
                return path;
            }

            return path.Trim('/', '\\');
        }

        public static string GetFileNameFromUnixPath(this string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            var segments = path.Split('/');
            return segments.Length > 0 ? segments[^1] : string.Empty;
        }

        public static string GetLastDirectoryFromPath(this string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;

            path = path.Replace('\\', '/').TrimEnd('/');

            if (path == "/")
                return "/";

            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

            return segments.Length > 0 ? segments[^1] : string.Empty;
        }

        public static string ReplaceExtension(this string path, string newExtension)
        {
            if (newExtension.StartsWith("."))
            {
                newExtension = newExtension[1..];
            }

            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

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

            return path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string GetCommonBasePath(this string path, params string[] paths) 
        {
            var combinedPaths = paths.ToList();
            combinedPaths.Add(path);
            return combinedPaths.GetCommonBasePath();
        }

        public static string GetCommonBasePath(this IEnumerable<string> directories)
        {
            if (!directories.Any())
            {
                return string.Empty;
            }

            string commonPath = directories.First();

            foreach (string path in directories)
            {
                while (!path.StartsWith(commonPath, StringComparison.OrdinalIgnoreCase))
                {
                    int lastSep = commonPath.LastIndexOf(Path.DirectorySeparatorChar);
                    if (lastSep <= 0)
                    {
                        return string.Empty;
                    }

                    commonPath = commonPath.Substring(0, lastSep);
                }
            }

            return commonPath;
        }

        public static List<string> GetRemainingPathSegments(this string fullPath, string basePath)
        {
            var full = fullPath.ToPathArray().Select(x => x.ToLower()).ToList();
            var baseParts = basePath.ToPathArray().Select(x => x.ToLower()).ToList();

            int start = FindStartIndexOfBasePath(full, baseParts);

            if (start == -1)
            {
                return [];
            }

            int remainingLength = full.Count - (start + baseParts.Count) - 1;

            if (remainingLength <= 0)
            {
                return [];
            }

            return full.GetRange(start + baseParts.Count, remainingLength);
        }

        private static int FindStartIndexOfBasePath(List<string> full, List<string> basePath)
        {
            for (int i = 0; i <= full.Count - basePath.Count; i++)
            {
                if (full.GetRange(i, basePath.Count).SequenceEqual(basePath))
                {
                    return i;
                }
            }

            return -1;
        }

        public static string DecodeWebEncodedPath(this string encodedPath)
        {
            if (string.IsNullOrEmpty(encodedPath))
            {
                return string.Empty;
            }

            return Uri.UnescapeDataString(encodedPath.Replace("+", " "));
        }

        public static void EnsureLocalPath(this string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        // === Validation Helpers ===

        public static bool IsValidUnixPath(this string path)
        {
            if (path.Contains('\\') || (path.Length >= 2 && path[1] == ':'))
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(path) || !path.StartsWith("/"))
            {
                return false;
            }

            if (path == "/")
            {
                return true;
            }

            var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                if (!IsValidUnixDirectoryName(part))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsValidUnixFilePath(this string path)
        {
            if (path.Contains('\\') || (path.Length >= 2 && path[1] == ':'))
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(path) || !path.StartsWith("/") || path == "/")
            {
                return false;
            }

            var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < parts.Length; i++)
            {
                bool isLast = i == parts.Length - 1;

                if (isLast)
                {
                    if (!IsValidUnixFileName(parts[i]))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!IsValidUnixDirectoryName(parts[i]))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static bool IsValidUnixFileName(this string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return false;
            }

            if (fileName == "." || fileName == "..")
            {
                return false;
            }

            if (fileName.Contains('/') || fileName.Contains('\0'))
            {
                return false;
            }

            foreach (char c in fileName)
            {
                if (!IsAllowedCharacter(c))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsValidUnixDirectoryName(this string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            if (name == "." || name == "..")
            {
                return false;
            }

            if (name.StartsWith("-") || name.Contains('/') || name.Contains('\0'))
            {
                return false;
            }

            foreach (char c in name)
            {
                if (!IsAllowedCharacter(c))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsSafeUnixDirectoryName(this string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            string forbiddenPattern = @"[\0:*?""<>|;&$`'\\]";

            if (Regex.IsMatch(name, forbiddenPattern) || name.StartsWith("-"))
            {
                return false;
            }

            if (name == "." || name == "..")
            {
                return false;
            }

            return true;
        }

        public static bool IsSafeWindowsDirectoryName(this string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            string forbiddenPattern = @"[\0:*?""<>|]";

            if (Regex.IsMatch(name, forbiddenPattern) || name.StartsWith("-"))
            {
                return false;
            }

            if (name == "." || name == "..")
            {
                return false;
            }

            return true;
        }

        private static bool IsAllowedCharacter(char c)
        {
            if (char.IsLetterOrDigit(c))
            {
                return true;
            }

            const string safe = " ._-()[]{}@!$%^&=+~'`,;:#&";
            const string retro = "©®™·•–—‘’“”øæß€¿¡★☆▓█▌✓✔←→↔∞";

            if (safe.Contains(c) || retro.Contains(c))
            {
                return true;
            }

            var category = char.GetUnicodeCategory(c);

            return category switch
            {
                UnicodeCategory.OtherSymbol => true,
                UnicodeCategory.MathSymbol => true,
                UnicodeCategory.ModifierSymbol => true,
                UnicodeCategory.CurrencySymbol => true,
                UnicodeCategory.LetterNumber => true,
                UnicodeCategory.OtherPunctuation => true,
                UnicodeCategory.DashPunctuation => true,
                UnicodeCategory.ConnectorPunctuation => true,
                UnicodeCategory.NonSpacingMark => true,
                UnicodeCategory.SpacingCombiningMark => true,
                UnicodeCategory.OtherNumber => true,
                _ => false,
            };
        }
    }
}
