using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Storage.Services
{
    public interface ICachedStorageService: IDisposable
    {
        IObservable<string> FileAdded { get; }

        void ClearCache();
        void ClearCache(string path);
        Task<StorageCacheItem?> GetDirectory(string path);
        Task<ILaunchableItem?> SaveFavorite(ILaunchableItem file);
        Task SaveFile(TeensyFileInfo fileInfo);
        Task QueuedSaveFile(TeensyFileInfo fileInfo);
        Task DeleteFile(IFileItem file, TeensyStorageType storageType);
        ILaunchableItem? GetRandomFile(params TeensyFileType[] fileTypes);
        IEnumerable<SongItem> SearchMusic(string searchText, int maxNumResults = 250);
        IEnumerable<ILaunchableItem> SearchFiles(string searchText);
        Task CacheAll();
        Task CacheAll(string path);
        void MarkIncompatible(ILaunchableItem launchItem);
        IEnumerable<GameItem> SearchGames(string searchText);
        IEnumerable<ILaunchableItem> Search(string searchText, params TeensyFileType[] fileTypes);
    }
}