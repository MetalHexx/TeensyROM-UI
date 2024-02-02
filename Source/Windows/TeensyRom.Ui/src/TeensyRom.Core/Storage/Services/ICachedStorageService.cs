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
        Task QueuedSaveFile(TeensyFileInfo fileInfo);
        Task DeleteFile(FileItem file, TeensyStorageType storageType);
        FileItem? GetRandomFile(params TeensyFileType[] fileTypes);
        IEnumerable<SongItem> SearchMusic(string searchText, int maxNumResults = 250);
        IEnumerable<FileItem> SearchFiles(string searchText);
        Task CacheAll();
        void MarkIncompatible(FileItem fileItem);
        IEnumerable<FileItem> SearchPrograms(string searchText);
    }
}