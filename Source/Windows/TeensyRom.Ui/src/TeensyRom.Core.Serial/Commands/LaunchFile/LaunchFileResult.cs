using TeensyRom.Core.Commands.File.LaunchFile;

namespace TeensyRom.Core.Commands.File.LaunchFile
{
    public class LaunchFileResult: TeensyCommandResult 
    {
        public LaunchFileResultType LaunchResult { get; set; }

        /// <summary>
        /// Indicates whether the file is compatible with TeensyROM hardware.
        /// This is computed based on the LaunchResult - SidError and ProgramError indicate incompatibility.
        /// </summary>
        public bool IsCompatible => LaunchResult is not (LaunchFileResultType.SidError or LaunchFileResultType.ProgramError);
    }
}