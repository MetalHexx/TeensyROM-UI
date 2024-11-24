using TeensyRom.Cli.Core.Commands;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Cli.Core.Storage.Services
{
    public interface ICachedStorageService: IDisposable
    {
        IObservable<IEnumerable<IFileItem>> FilesAdded { get; }
        void ClearCache();
        void ClearCache(string path);
        Task<StorageCacheItem?> GetDirectory(string path);
        Task<ILaunchableItem?> SaveFavorite(ILaunchableItem file);
        void BanFile(ILaunchableItem file);
        Task<SaveFilesResult> SaveFiles(IEnumerable<FileTransferItem> files);
        Task DeleteFile(IFileItem file, TeensyStorageType storageType);
        ILaunchableItem? GetRandomFile(StorageScope scope, string scopePath, params TeensyFileType[] fileTypes);
        Task CacheAll();
        Task CacheAll(string path);
        void MarkIncompatible(ILaunchableItem launchItem);
        IEnumerable<ILaunchableItem> Search(string searchText, params TeensyFileType[] fileTypes);
        Task RemoveFavorite(ILaunchableItem file);
        void SwitchStorage(TeensyStorageType storageType);
    }
}