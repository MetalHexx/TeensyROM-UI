using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Core.Storage
{
    public interface IStorageCacheItem
    {
        List<DirectoryItem> Directories { get; set; }
        List<IFileItem> Files { get; set; }
        string Path { get; set; }

        void DeleteFile(string path);
        void InsertSubdirectory(DirectoryItem directory);
        List<IStorageItem> ToList();
        void UpsertFile(IFileItem fileItem);
    }
}