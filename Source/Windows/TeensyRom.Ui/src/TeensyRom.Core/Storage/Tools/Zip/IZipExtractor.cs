using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Storage.Tools.Zip
{
    public interface IZipExtractor
    {
        void ClearOutputDirectory();
        ExtractionResult Extract(FileTransferItem zipItem);
    }
}
