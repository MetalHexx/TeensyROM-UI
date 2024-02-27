using System.IO.Compression;
using System.Reflection;
using TeensyRom.Core.Common;

namespace TeensyRom.Core.Common.Abstractions
{
    public abstract class UnpackService(string _imagePath, string _zipFileName) : IUnpackAssetService
    {
        private string _assetFullPath => Path.Combine(Assembly.GetExecutingAssembly().GetPath(), _imagePath);
        private string _zipFullPath => Path.Combine(_assetFullPath, _zipFileName);

        public void UnpackImages()
        {
            if (!File.Exists(_zipFullPath)) return;

            ZipFile.ExtractToDirectory(_zipFullPath, _assetFullPath, true);
            File.Delete(_zipFullPath);
        }
    }
}
