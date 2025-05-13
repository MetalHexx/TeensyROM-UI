using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Core.Settings
{
    public readonly record struct TeensyTarget
    (
        TeensyFileType Type,
        TeensyFilterType FilterType,
        string DisplayName,
        string Extension
    );
}