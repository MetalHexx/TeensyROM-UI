using TeensyRom.Core.Storage;

namespace TeensyRom.Core.Settings
{
    public class TeensyTarget
    {
        public TeensyFileType Type { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public string TargetPath { get; set; } = string.Empty;
    }
}