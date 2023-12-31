using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands
{
    public class GetDirectoryResult : CommandResult
    {
        public DirectoryContent? DirectoryContent { get; set; }
    }
}
