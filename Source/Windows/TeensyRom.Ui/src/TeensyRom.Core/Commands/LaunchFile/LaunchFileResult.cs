using TeensyRom.Core.Commands;

namespace TeensyRom.Core.Commands.LaunchFile
{
    public class LaunchFileResult : TeensyCommandResult
    {
        public LaunchFileResultType LaunchResult { get; set; }
    }
}