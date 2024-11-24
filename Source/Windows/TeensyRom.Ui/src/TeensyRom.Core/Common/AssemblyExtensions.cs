using System.Reflection;

namespace TeensyRom.Core.Common
{

    public static class AssemblyExtensions
    {
        /// <summary>
        /// Get the full path of the assembly
        /// </summary>
        public static string GetPath(this Assembly assembly)
        {
            var location = assembly.Location;
            return Path.GetDirectoryName(location) ?? string.Empty;
        }

        public static string GetVersion(this Assembly assembly)
        {
            string version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "Unknown version";
            var plusIndex = version.IndexOf('+');
            version = plusIndex > -1 ? version.Substring(0, plusIndex) : version;
            return version;
        }
    }
}
