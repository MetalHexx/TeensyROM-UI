using TeensyRom.Core.Music;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Storage.Services
{
    public interface ICachedStorageService: IDisposable
    {
        IObservable<string> DirectoryUpdated { get; }

        void ClearCache();
        void ClearCache(string path);
        Task<StorageCacheItem?> GetDirectory(string path);
        Task<FileItem?> SaveFavorite(FileItem item);
        Task SaveFile(TeensyFileInfo fileInfo);
    }
}