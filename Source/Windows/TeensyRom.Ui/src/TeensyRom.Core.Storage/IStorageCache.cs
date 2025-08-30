
using System.Reactive;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Settings;
using TeensyRom.Core.ValueObjects;

namespace TeensyRom.Core.Storage
{
    public interface IStorageCache
    {
        IObservable<Unit> StorageReady { get; }

        void DeleteDirectory(DirectoryPath path);
        void DeleteDirectoryWithChildren(DirectoryPath path);
        void DeleteFile(FilePath path);
        void EnsureParents(DirectoryPath path);
        IStorageCacheItem? GetByDirPath(DirectoryPath path);
        List<FileItem> GetFileByName(string name);
        FileItem? GetFileByPath(FilePath filePath);
        void UpsertDirectory(DirectoryPath path, IStorageCacheItem directory);
        void UpsertFile(FileItem fileItem);
        void Clear();
        void EnsureFavorites();
        IEnumerable<LaunchableItem> Search(string searchText, IEnumerable<DirectoryPath> excludePaths, List<string> stopSearchWords, SearchWeights searchWeights, params TeensyFileType[] fileTypes);
        LaunchableItem? GetRandomFile(StorageScope scope, DirectoryPath scopePath, IEnumerable<DirectoryPath> excludePaths, params TeensyFileType[] fileTypes);
        void WriteToDisk();
        void ClearCache();
        int GetCacheSize();
        void EnsurePlaylists();
        List<LaunchableItem> GetFavoriteFiles();
        List<LaunchableItem> GetPlaylistFiles();
        FileItem? FindParentFile(FileItem item);
        List<FileItem> FindSiblings(FileItem item);
    }
}