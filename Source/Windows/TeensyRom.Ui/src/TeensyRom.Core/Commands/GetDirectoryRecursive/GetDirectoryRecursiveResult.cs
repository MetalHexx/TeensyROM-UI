using TeensyRom.Core.Commands.Common;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands
{
    public class GetDirectoryRecursiveResult : TeensyCommandResult
    {
        public List<DirectoryContent?> DirectoryContent { get; set; } = new();
        public GetDirectoryErrorCodeType ErrorCode { get; set; }
    }
}
