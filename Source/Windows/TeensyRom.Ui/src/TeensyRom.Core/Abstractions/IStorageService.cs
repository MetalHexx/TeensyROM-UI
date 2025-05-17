using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Storage;

namespace TeensyRom.Core.Abstractions
{
    public interface IStorageService
    {
        Task<bool> CacheAll();
        Task<bool> Cache(string path);
        public void ClearCache();
        public void ClearCache(string path);
        public Task<IFileItem?> GetFile(string filePath);
        Task<IStorageCacheItem?> GetDirectory(string directoryPath);
    }
}
