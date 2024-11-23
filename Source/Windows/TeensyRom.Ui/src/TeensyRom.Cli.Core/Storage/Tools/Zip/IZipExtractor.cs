using TeensyRom.Cli.Core.Storage.Entities;

namespace TeensyRom.Cli.Core.Storage.Tools.Zip
{
    public interface IZipExtractor
    {
        void ClearOutputDirectory();
        ExtractionResult Extract(FileTransferItem zipItem);
    }
}
