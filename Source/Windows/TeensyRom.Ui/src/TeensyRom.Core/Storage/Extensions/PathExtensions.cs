namespace TeensyRom.Core.Storage.Extensions
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
    }
}