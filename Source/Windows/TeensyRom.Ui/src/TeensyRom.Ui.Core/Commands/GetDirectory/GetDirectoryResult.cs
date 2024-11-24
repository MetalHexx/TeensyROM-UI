using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Ui.Core.Commands
{
    public class GetDirectoryResult : TeensyCommandResult
    {
        public DirectoryContent? DirectoryContent { get; set; }
    }
}
