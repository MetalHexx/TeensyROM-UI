using TeensyRom.Core.ValueObjects;

namespace TeensyRom.Core.Entities.Storage
{
    /// <summary>
    /// DTO for copying existing files to other locations on TR storage
    /// </summary>
    public class CopyFileItem
    {
        public LaunchableItem SourceItem { get; set; } = default!;
        public DirectoryPath TargetPath { get; set; } = new DirectoryPath(string.Empty);

        public CopyFileItem() { }

        public CopyFileItem(LaunchableItem item, DirectoryPath targetpath)
        {
            SourceItem = item;
            TargetPath = targetpath;
        }
    }
}