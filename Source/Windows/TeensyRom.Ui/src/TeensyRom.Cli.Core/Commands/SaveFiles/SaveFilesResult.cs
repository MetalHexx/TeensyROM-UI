using TeensyRom.Cli.Core.Storage.Entities;

namespace TeensyRom.Cli.Core.Commands
{
    public class SaveFilesResult : TeensyCommandResult 
    {
        public List<FileTransferItem> SuccessfulFiles { get; set; } = [];        
        public List<FileTransferItem> FailedFiles { get; set; } = [];
    }
}