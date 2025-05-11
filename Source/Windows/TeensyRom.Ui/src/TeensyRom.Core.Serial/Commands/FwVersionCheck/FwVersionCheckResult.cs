using TeensyRom.Core.Commands;

namespace TeensyRom.Core.Serial.Commands.FwVersionCheck
{
    public class FwVersionCheckResult : TeensyCommandResult 
    {
        public Version? Version { get; set; }
        public bool IsTeensyRom { get; set; }
        public bool IsCompatible { get; set; }
    }
}
