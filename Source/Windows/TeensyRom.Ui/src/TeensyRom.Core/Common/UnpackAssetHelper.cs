using System.IO.Compression;
using System.Reflection;

namespace TeensyRom.Core.Common
{
    public static class AssetHelper
    {
        public static void UnpackImages(string imagePath, string zipFileName)
        {
            var assetFullPath = Path.Combine(Assembly.GetExecutingAssembly().GetPath(), imagePath);
            var zipFullPath = Path.Combine(assetFullPath, zipFileName);

            if (!File.Exists(zipFullPath)) return;

            ZipFile.ExtractToDirectory(zipFullPath, assetFullPath, true);
            File.Delete(zipFullPath);
        }
    }
}
