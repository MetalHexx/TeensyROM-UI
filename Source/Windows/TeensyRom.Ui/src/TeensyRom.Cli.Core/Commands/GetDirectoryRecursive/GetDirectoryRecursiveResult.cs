using TeensyRom.Cli.Core.Storage.Entities;

namespace TeensyRom.Cli.Core.Commands
{
    public class GetDirectoryRecursiveResult : TeensyCommandResult
    {
        public List<DirectoryContent?> DirectoryContent { get; set; } = new();
    }
}
