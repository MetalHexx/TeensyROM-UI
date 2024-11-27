using TeensyRom.Core.Commands;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands.SaveFiles
{
    public class SaveFilesResult : TeensyCommandResult
    {
        public List<FileTransferItem> SuccessfulFiles { get; set; } = [];
        public List<FileTransferItem> FailedFiles { get; set; } = [];
    }
}