using TeensyRom.Cli.Core.Storage.Entities;

namespace TeensyRom.Cli.Core.Commands
{
    public class GetDirectoryResult : TeensyCommandResult
    {
        public DirectoryContent? DirectoryContent { get; set; }
    }
}
