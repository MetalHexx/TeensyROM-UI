using TeensyRom.Core.Commands;

namespace TeensyRom.Core.Commands.GetFile
{
    public class GetFileResult : TeensyCommandResult
    {
        public byte[] FileData { get; set; } = default!;
    }
}