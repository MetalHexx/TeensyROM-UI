using System.Reactive;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.ValueObjects;

namespace TeensyRom.Core.Storage
{
    public interface ICachedStorageService : IDisposable
    {
        IObservable<IEnumerable<FileItem>> FilesAdded { get; }
        IObservable<IEnumerable<FileItem>> FilesChanged { get; }
        IObservable<Unit> StorageReady { get; }
        IObservable<IEnumerable<FileItem>> FilesDeleted { get; }

        void ClearCache();
        void ClearCache(DirectoryPath path);
        Task<IStorageCacheItem?> GetDirectory(DirectoryPath path);
        Task<LaunchableItem?> SaveFavorite(LaunchableItem file);
        void SaveFiles(IEnumerable<FileItem> files);
        Task DeleteFile(FileItem file, TeensyStorageType storageType);
        LaunchableItem? GetRandomFile(StorageScope scope, DirectoryPath scopePath, params TeensyFileType[] fileTypes);
        Task CacheAll();
        Task CacheAll(DirectoryPath path);
        void MarkIncompatible(LaunchableItem launchItem);
        IEnumerable<LaunchableItem> Search(string searchText, params TeensyFileType[] fileTypes);
        Task RemoveFavorite(LaunchableItem file);
        Task CopyFiles(List<CopyFileItem> fileItems);
        int GetCacheSize();
        Task UpsertFiles(IEnumerable<FileItem> files);
    }
}