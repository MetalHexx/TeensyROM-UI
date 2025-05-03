using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Core.Commands
{
    public class GetDirectoryRecursiveResult : TeensyCommandResult
    {
        public List<DirectoryContent?> DirectoryContent { get; set; } = new();
    }
}
