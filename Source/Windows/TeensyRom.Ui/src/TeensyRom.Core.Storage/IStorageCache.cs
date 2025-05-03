
using System.Reactive;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Settings;

namespace TeensyRom.Core.Storage
{
    public interface IStorageCache
    {
        IObservable<Unit> StorageReady { get; }

        void DeleteDirectory(string path);
        void DeleteDirectoryWithChildren(string path);
        void DeleteFile(string path);
        StorageCacheItem EnsureParents(string path);
        StorageCacheItem? GetByDirPath(string path);
        List<IFileItem> GetFileByName(string name);
        IFileItem? GetFileByPath(string filePath);
        void UpsertDirectory(string path, StorageCacheItem directory);
        void UpsertFile(IFileItem fileItem);
        void Clear();
        void EnsureFavorites(List<string> favPaths);
        IEnumerable<ILaunchableItem> Search(string searchText, IEnumerable<string> excludePaths, List<string> stopSearchWords, SearchWeights searchWeights, params TeensyFileType[] fileTypes);
        ILaunchableItem? GetRandomFile(StorageScope scope, string scopePath, IEnumerable<string> excludePaths, params TeensyFileType[] fileTypes);
        void WriteToDisk();
        void ClearCache();
        int GetCacheSize();
    }
}