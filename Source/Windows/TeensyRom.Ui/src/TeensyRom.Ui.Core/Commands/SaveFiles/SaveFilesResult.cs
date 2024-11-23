using TeensyRom.Ui.Core.Storage.Entities;

namespace TeensyRom.Ui.Core.Commands
{
    public class SaveFilesResult : TeensyCommandResult 
    {
        public List<FileTransferItem> SuccessfulFiles { get; set; } = [];        
        public List<FileTransferItem> FailedFiles { get; set; } = [];
    }
}