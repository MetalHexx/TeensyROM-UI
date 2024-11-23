using TeensyRom.Cli.Core.Commands.File.LaunchFile;

namespace TeensyRom.Cli.Core.Commands.File.LaunchFile
{
    public class LaunchFileResult: TeensyCommandResult 
    {
        public LaunchFileResultType LaunchResult { get; set; }
    }
}