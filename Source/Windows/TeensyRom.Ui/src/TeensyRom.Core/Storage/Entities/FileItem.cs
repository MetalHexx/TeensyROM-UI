using TeensyRom.Core.Common;

namespace TeensyRom.Core.Storage.Entities
{
    public class FileItem : StorageItem 
    {
        public string Id => $"{Size}{Path.GetFileNameFromPath()}";
        public string ShareUrl { get; set; }
        public string MetadataSource { get; set; }
        public FileItem() { }
        public FileItem(string path)
        {
            Path = path;
            Name = Path.GetFileNameFromPath();            
        }
        public TeensyFileType FileType => Path.GetUnixFileExtension().GetFileType();

        public virtual FileItem Clone()
        {
            return new FileItem
            {
                Name = Name,
                Path = Path,
                Size = Size,
                IsFavorite = IsFavorite,
                IsSelected = IsSelected
            };
        }
    }
}
