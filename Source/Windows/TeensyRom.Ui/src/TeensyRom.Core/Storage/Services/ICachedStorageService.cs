using TeensyRom.Core.Music;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Storage.Services
{
    public interface ICachedStorageService: IDisposable
    {
        void ClearCache();
        void ClearCache(string path);
        Task<StorageCacheItem?> GetDirectory(string path);
        Task<FileItem?> SaveFavorite(FileItem item);
    }
}