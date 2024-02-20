using TeensyRom.Core.Common;

namespace TeensyRom.Core.Storage.Entities
{
    public class FileItem : StorageItem, IFileItem
    {   
        public string Title { get; set; } = string.Empty;
        public string Creator { get; set; } = string.Empty;
        public string ReleaseInfo { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ShareUrl { get; set; } = string.Empty;
        public string MetadataSource { get; set; } = string.Empty;
        public string Id => $"{Size}{Path.GetFileNameFromPath()}";
        public TeensyFileType FileType => Path.GetUnixFileExtension().GetFileType();

        public virtual FileItem Clone()
        {
            return new FileItem
            {
                Name = Name,
                Path = Path,
                Size = Size,
                IsFavorite = IsFavorite,
                Title = Title,
                Creator = Creator,
                ReleaseInfo = ReleaseInfo,
                Description = Description
            };
        }
    }
}
