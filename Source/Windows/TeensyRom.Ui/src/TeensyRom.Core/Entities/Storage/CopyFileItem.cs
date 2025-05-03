namespace TeensyRom.Core.Entities.Storage
{
    /// <summary>
    /// DTO for copying existing files to other locations on TR storage
    /// </summary>
    public class CopyFileItem
    {
        public ILaunchableItem SourceItem { get; set; } = default!;
        public string TargetPath { get; set; } = string.Empty;

        public CopyFileItem() { }

        public CopyFileItem(ILaunchableItem item, string targetpath)
        {
            SourceItem = item;
            TargetPath = targetpath;
        }
    }
}