using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Assets.Tools.Vice
{
    public interface ID64Extractor
    {
        void ClearOutputDirectory();
        D64ExtractionResult Extract(FileTransferItem d64);
    }
}