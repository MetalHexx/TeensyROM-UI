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
        Task<int> SaveFiles(IEnumerable<TeensyFileInfo> files);
        Task QueuedSaveFile(TeensyFileInfo fileInfo);
        Task DeleteFile(IFileItem file, TeensyStorageType storageType);
        ILaunchableItem? GetRandomFile(params TeensyFileType[] fileTypes);
        Task CacheAll();
        Task CacheAll(string path);
        void MarkIncompatible(ILaunchableItem launchItem);
        IEnumerable<ILaunchableItem> Search(string searchText, params TeensyFileType[] fileTypes);
    }
}