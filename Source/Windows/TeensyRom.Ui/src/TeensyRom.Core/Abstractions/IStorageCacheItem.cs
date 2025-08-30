using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.ValueObjects;

namespace TeensyRom.Core.Storage
{
    public interface IStorageCacheItem
    {
        List<DirectoryItem> Directories { get; set; }
        List<FileItem> Files { get; set; }
        DirectoryPath Path { get; set; }

        void DeleteFile(FilePath path);
        void InsertSubdirectory(DirectoryItem directory);
        List<StorageItem> ToList();
        void UpsertFile(FileItem fileItem);
    }
}