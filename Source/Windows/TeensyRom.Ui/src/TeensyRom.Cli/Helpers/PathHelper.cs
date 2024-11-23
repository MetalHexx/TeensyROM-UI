using System.Reflection;

namespace TeensyRom.Cli.Helpers
{
    internal static class PathHelper
    {
        public static string GetAssemblyRootedPath(this string path)
        {
            var assemblyLoc = Assembly.GetExecutingAssembly().Location;
            var assemblyPath = Path.GetDirectoryName(assemblyLoc) ?? string.Empty;
            return Path.Combine(assemblyPath, path);
        }

        public static string GetCwdRootedPath(this string path)
        {
            var cwd = Directory.GetCurrentDirectory();
            return Path.Combine(cwd, path);
        }
    }
}
