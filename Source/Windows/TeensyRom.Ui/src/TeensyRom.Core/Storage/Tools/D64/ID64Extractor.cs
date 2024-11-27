using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Storage.Tools.D64
{
    public interface ID64Extractor
    {
        void ClearOutputDirectory();
        ExtractionResult Extract(FileTransferItem d64Item);
    }
}