using TeensyRom.Core.Commands.File.LaunchFile;

namespace TeensyRom.Core.Commands.File.LaunchFile
{
    public class LaunchFileResult: TeensyCommandResult 
    {
        public LaunchFileResultType LaunchResult { get; set; }
    }
}