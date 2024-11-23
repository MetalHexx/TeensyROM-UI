using TeensyRom.Ui.Core.Storage.Entities;

namespace TeensyRom.Ui.Core.Storage.Tools.Zip
{
    public interface IZipExtractor
    {
        void ClearOutputDirectory();
        ExtractionResult Extract(FileTransferItem zipItem);
    }
}
