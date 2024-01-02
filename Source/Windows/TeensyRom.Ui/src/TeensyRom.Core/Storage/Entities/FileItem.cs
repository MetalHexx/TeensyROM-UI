using TeensyRom.Core.Common;

namespace TeensyRom.Core.Storage.Entities
{
    public class FileItem : StorageItem 
    {
        public TeensyFileType FileType => Path.GetUnixFileExtension().GetFileType();

        public FileItem Clone()
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
