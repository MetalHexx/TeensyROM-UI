﻿namespace TeensyRom.Core.Common
{
    public static class PathExtensions
    {
        public static string EnsureUnixPathEnding(this string path) => path.EndsWith("/") ? path : path + "/";
        public static string UnixPathCombine(this string path, params string[] paths)
        {
            if (!path.EndsWith("/"))
            {
                path += "/";
            }
            string combined = path + string.Join("/", paths);
            return combined.Replace("//", "/");
        }

        public static string GetParentDirectory(this string path)
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
    }
}