using System.IO.Compression;
using System.Reflection;

namespace TeensyRom.Ui.Core.Common
{
    public static class AssetHelper
    {
        public static void UnpackAssets(string assetPath, string zipFileName)
        {
            var assetFullPath = Path.Combine(Assembly.GetExecutingAssembly().GetPath(), assetPath);
            var zipFullPath = Path.Combine(assetFullPath, zipFileName);

            if (!File.Exists(zipFullPath)) return;

            ZipFile.ExtractToDirectory(zipFullPath, assetFullPath, true);
            File.Delete(zipFullPath);
        }
    }
}
