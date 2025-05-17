using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Serial.Commands.Common;

namespace TeensyRom.Core.Commands
{
    public class GetDirectoryRecursiveResult : TeensyCommandResult
    {
        public List<DirectoryContent?> DirectoryContent { get; set; } = new();
        public GetDirectoryErrorCode ErrorCode { get; set; }
    }
}
