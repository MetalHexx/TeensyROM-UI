using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands
{
    public class GetDirectoryResult : TeensyCommandResult
    {
        public DirectoryContent? DirectoryContent { get; set; }
    }
}
