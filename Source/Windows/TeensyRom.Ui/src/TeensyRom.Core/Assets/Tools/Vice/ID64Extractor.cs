using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Assets.Tools.Vice
{
    public interface ID64Extractor: IDisposable
    {
        void ClearOutputDirectory();
        IEnumerable<FileTransferItem> Extract(IEnumerable<FileTransferItem> files);
    }
}