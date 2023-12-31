using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands
{
    public class GetDirectoryResponse : CommandResult
    {
        public DirectoryContent? DirectoryContent { get; set; }
    }
}
