using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage;

namespace TeensyRom.Core.Abstractions
{
    public interface IStorageService
    {
        Task<bool> CacheAll(CancellationToken ct);
        Task<bool> Cache(string path, CancellationToken ct);
        public void ClearCache();
        public void ClearCache(string path);
        public Task<IFileItem?> GetFile(string filePath);
        Task<IStorageCacheItem?> GetDirectory(string directoryPath);
        ILaunchableItem? GetRandomFile(StorageScope scope, string scopePath, TeensyFilterType filterType);
    }
}
