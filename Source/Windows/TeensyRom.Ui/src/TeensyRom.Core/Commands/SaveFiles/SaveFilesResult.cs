using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands
{
    public class SaveFilesResult : TeensyCommandResult 
    {
        public List<TeensyFileInfo> SuccessfulFiles { get; set; } = [];        
        public List<TeensyFileInfo> FailedFiles { get; set; } = [];
    }
}