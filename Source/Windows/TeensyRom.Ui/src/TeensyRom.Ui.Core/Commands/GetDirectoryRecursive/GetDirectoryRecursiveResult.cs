using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Ui.Core.Commands
{
    public class GetDirectoryRecursiveResult : TeensyCommandResult
    {
        public List<DirectoryContent?> DirectoryContent { get; set; } = new();
    }
}
