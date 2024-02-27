using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Common;
using TeensyRom.Core.Music.Sid;

namespace TeensyRom.Core.Music.Images
{
    public interface IUnpackAssetService
    {
        void UnpackImages();
    }

    public class MusicUnpackService : IUnpackAssetService
    {
        private string _musicImagePath => Path.Combine(Assembly.GetExecutingAssembly().GetPath(), SidConstants.Musician_Image_Local_Path);
        private string _composerZipFilePath => Path.Combine(_musicImagePath, "Composers.zip");
        public void UnpackImages()
        {
            if (!File.Exists(_composerZipFilePath)) return;

            ZipFile.ExtractToDirectory(_composerZipFilePath, _musicImagePath, true);
            File.Delete(_composerZipFilePath);
        }
    }
}
