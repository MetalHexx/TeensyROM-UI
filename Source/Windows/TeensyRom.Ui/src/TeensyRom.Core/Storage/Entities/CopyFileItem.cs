namespace TeensyRom.Core.Storage.Entities
{
    /// <summary>
    /// DTO for copying existing files to other locations on TR storage
    /// </summary>
    public class CopyFileItem 
    {
        public ILaunchableItem SourceItem { get; set; }
        public string TargetPath { get; set; } = string.Empty;

        public CopyFileItem(ILaunchableItem item, string targetpath)
        {
            SourceItem = item;
            TargetPath = targetpath;
        }
    }
}