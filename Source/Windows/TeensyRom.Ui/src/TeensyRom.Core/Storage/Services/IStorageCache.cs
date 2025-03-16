
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Storage.Services
{
    public interface IStorageCache
    {
        void DeleteDirectory(string path);
        void DeleteDirectoryWithChildren(string path);
        void DeleteFile(string path);
        StorageCacheItem EnsureParents(string path);
        StorageCacheItem? GetByDirPath(string path);
        List<IFileItem> GetFileByName(string name);
        IFileItem? GetFileByPath(string filePath);
        void SetBanLists(List<string> bannedFolders, List<string> bannedFiles);
        void UpsertDirectory(string path, StorageCacheItem directory);
        void UpsertFile(IFileItem fileItem);
        void Clear();
        void EnsureFavorites(List<string> favPaths);
        IEnumerable<ILaunchableItem> Search(string searchText, List<string> favPaths, List<string> stopSearchWords, SearchWeights searchWeights, params TeensyFileType[] fileTypes);
        ILaunchableItem? GetRandomFile(StorageScope scope, string scopePath, params TeensyFileType[] fileTypes);
    }
}