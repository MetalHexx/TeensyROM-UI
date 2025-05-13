namespace TeensyRom.Core.Settings
{
    public readonly record struct TeensyFilter
    (
        TeensyFilterType Type,
        string DisplayName,
        string Icon
    );
}