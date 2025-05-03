using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Core.Commands
{
    public class GetDirectoryResult : TeensyCommandResult
    {
        public DirectoryContent? DirectoryContent { get; set; }
    }
}
