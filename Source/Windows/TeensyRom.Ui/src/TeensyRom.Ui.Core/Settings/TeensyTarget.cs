using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Ui.Core.Settings
{
    public class TeensyTarget
    {        
        public TeensyFileType Type { get; set; }
        public TeensyFilterType FilterType { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
    }
}