using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage;
using TeensyRom.Core.ValueObjects;

namespace TeensyRom.Core.Abstractions
{
    public interface IStorageService
    {
        Task<bool> CacheAll(CancellationToken ct);
        Task<bool> Cache(DirectoryPath path, CancellationToken ct);
        public void ClearCache();
        public void ClearCache(DirectoryPath path);
        public Task<FileItem?> GetFile(FilePath filePath);
        Task<IStorageCacheItem?> GetDirectory(DirectoryPath directoryPath);
        LaunchableItem? GetRandomFile(StorageScope scope, DirectoryPath scopePath, TeensyFilterType filterType);
        IEnumerable<LaunchableItem> Search(string searchText, TeensyFilterType filterType = TeensyFilterType.All, int skip = 0, int take = 50);
        Task<LaunchableItem?> SaveFavorite(LaunchableItem launchItem, TeensyStorageType storageType, CancellationToken ct);
        Task<bool> RemoveFavorite(LaunchableItem file, TeensyStorageType storageType, CancellationToken ct);
    }
}
