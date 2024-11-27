using TeensyRom.Core.Commands;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands.GetDirectory
{
    public class GetDirectoryResult : TeensyCommandResult
    {
        public DirectoryContent? DirectoryContent { get; set; }
    }
}
