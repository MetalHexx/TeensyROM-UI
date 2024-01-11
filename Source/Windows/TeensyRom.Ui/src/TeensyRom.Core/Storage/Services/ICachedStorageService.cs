using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Storage.Services
{
    public interface ICachedStorageService: IDisposable
    {
        IObservable<string> DirectoryUpdated { get; }

        void ClearCache();
        void ClearCache(string path);
        Task<StorageCacheItem?> GetDirectory(string path);
        Task<FileItem?> SaveFavorite(FileItem file);
        Task SaveFile(TeensyFileInfo fileInfo);
        Task DeleteFile(FileItem file, TeensyStorageType storageType);
        FileItem? GetRandomFile(params TeensyFileType[] fileTypes);
        IEnumerable<SongItem> SearchMusic(string keyword);
        Task CacheAll();
    }
}