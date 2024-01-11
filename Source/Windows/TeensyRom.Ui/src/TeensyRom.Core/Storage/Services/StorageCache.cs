using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Concurrency;
using TeensyRom.Core.Common;
using TeensyRom.Core.Storage.Entities;
using static System.Net.Mime.MediaTypeNames;

namespace TeensyRom.Core.Storage.Services
{
    /// <summary>
    /// - random automatically puts you into shuffle mode
    /// - clicking next in shuffle mode will be added to a history list to remember your previous track
    /// - hitting previous in shuffle mode will now bring to the previous item in the history list
    /// - hitting previous in shuffle mode when you're at the beginning of the history will do nothing.
    /// - clicking a song in your current directory will pull you out of shuffle mode and clear history.Clicking next or previous will go to the next or previous song in the current directory.
    /// </summary>
    public class StorageCache : Dictionary<string, StorageCacheItem>
    {
        public void UpsertDirectory(string path, StorageCacheItem directory)
        {
            DeleteDirectory(path);
            Insert(path, directory);
        }

        public void UpsertFile(FileItem fileItem)
        {
            var fileParentDir = EnsureParents(fileItem.Path);

            fileParentDir!.UpsertFile(fileItem);
            UpsertDirectory(fileParentDir.Path, fileParentDir);
        }

        public StorageCacheItem EnsureParents(string path)
        {
            var parentPath = CleanPath(path.GetUnixParentPath());
            var fileParentDir = Get(parentPath);

            if (fileParentDir is null)
            {
                fileParentDir = new StorageCacheItem
                {
                    Path = parentPath,
                    Directories = [],
                    Files = []
                };
                Insert(fileParentDir.Path, fileParentDir);
            }
            if (string.IsNullOrWhiteSpace(parentPath)) return fileParentDir;

            var grandParent = EnsureParents(parentPath);

            grandParent.InsertSubdirectory(new DirectoryItem
            {
                Name = fileParentDir.Path.GetLastDirectoryFromPath(),
                Path = fileParentDir.Path,
            });
            return fileParentDir;
        }

        private void Insert(string path, StorageCacheItem cacheItem)
        {
            var cleanPath = CleanPath(path);
            TryAdd(cleanPath, cacheItem);
        }

        public void DeleteDirectory(string path)
        {
            var cleanPath = CleanPath(path);
            var dir = Get(cleanPath);

            if (dir is null) return;

            Remove(cleanPath);
        }

        public void DeleteDirectoryWithChildren(string path)
        {
            var currentDir = Get(path);

            if (currentDir is null) return;

            foreach (var directory in currentDir.Directories)
            {
                DeleteDirectoryWithChildren(directory.Path);
            }
            DeleteDirectory(currentDir.Path);
        }

        public StorageCacheItem? Get(string path)
        {
            var cleanPath = CleanPath(path);

            if (!TryGetValue(cleanPath, out var item)) return null;

            return item;
        }

        public void DeleteFile(string path)
        {
            var cleanPath = CleanPath(path);
            var parentPath = cleanPath.GetUnixParentPath();
            var parentDir = Get(parentPath);

            if (parentDir is null) return;

            parentDir.DeleteFile(path);
        }

        public List<FileItem> FindFile(string name)
        {
            return this.SelectMany(c => c.Value.Files)
                .Where(f => f.Name.Equals(name))
                .ToList();
        }

        private static string CleanPath(string path) => path
            .RemoveLeadingAndTrailingSlash();

        internal IEnumerable<SongItem> Search(string keyword)
        {
            return this
                .SelectMany(c => c.Value.Files)
                .OfType<SongItem>()
                .Where(song =>
                    song.ArtistName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    song.SongName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    song.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }
    }
}