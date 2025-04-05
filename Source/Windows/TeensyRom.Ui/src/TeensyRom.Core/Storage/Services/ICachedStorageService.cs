using System.Reactive;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Commands.DeleteFile;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Storage.Services
{
    public interface ICachedStorageService: IDisposable
    {
        IObservable<IEnumerable<IFileItem>> FilesAdded { get; }
        IObservable<IEnumerable<IFileItem>> FilesCopied { get; }
        IObservable<Unit> StorageReady { get; }
        IObservable<IEnumerable<IFileItem>> FilesDeleted { get; }

        void ClearCache();
        void ClearCache(string path);
        Task<StorageCacheItem?> GetDirectory(string path);
        Task<ILaunchableItem?> SaveFavorite(ILaunchableItem file);
        Task<SaveFilesResult> SaveFiles(IEnumerable<FileTransferItem> files);
        Task DeleteFile(IFileItem file, TeensyStorageType storageType);
        ILaunchableItem? GetRandomFile(StorageScope scope, string scopePath, params TeensyFileType[] fileTypes);
        Task CacheAll();
        Task CacheAll(string path);
        void MarkIncompatible(ILaunchableItem launchItem);
        IEnumerable<ILaunchableItem> Search(string searchText, params TeensyFileType[] fileTypes);
        Task RemoveFavorite(ILaunchableItem file);
        Task CopyFiles(List<CopyFileItem> fileItems);
        int GetCacheSize();
    }
}