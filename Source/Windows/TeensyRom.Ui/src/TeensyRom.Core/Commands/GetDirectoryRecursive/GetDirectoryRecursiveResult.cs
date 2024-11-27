using TeensyRom.Core.Commands;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands.GetDirectoryRecursive
{
    public class GetDirectoryRecursiveResult : TeensyCommandResult
    {
        public List<DirectoryContent?> DirectoryContent { get; set; } = new();
    }
}
