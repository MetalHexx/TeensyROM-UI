using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands
{
    public class GetDirectoryRecursiveResult : CommandResult
    {
        public List<DirectoryContent?> DirectoryContent { get; set; } = new();
    }
}
