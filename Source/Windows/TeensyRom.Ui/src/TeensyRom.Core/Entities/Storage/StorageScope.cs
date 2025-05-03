using System.ComponentModel;
using System.Net.NetworkInformation;

namespace TeensyRom.Core.Entities.Storage
{
    public enum StorageScope
    {
        [Description("Selected Storage")]
        Storage,
        [Description("Directory (Deep)")]
        DirDeep,
        [Description("Directory (Shallow)")]
        DirShallow
    }
}