using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Core.Storage.Tools.Zip
{
    public interface IZipExtractor
    {
        void ClearOutputDirectory();
        ExtractionResult Extract(FileTransferItem zipItem);
    }
}
