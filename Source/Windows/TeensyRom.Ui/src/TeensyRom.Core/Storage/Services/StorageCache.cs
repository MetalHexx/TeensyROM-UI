using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Concurrency;
using TeensyRom.Core.Common;
using TeensyRom.Core.Settings;
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
        private List<string> _bannedFolders = [];
        private List<string> _bannedFiles = [];

        public StorageCache() { }
        public StorageCache(List<string> bannedFolders, List<string> bannedFiles)
        {
            _bannedFolders = bannedFolders;
            _bannedFiles = bannedFiles;
        }
        public void SetBanLists(List<string> bannedFolders, List<string> bannedFiles)
        {
            _bannedFolders = bannedFolders;
            _bannedFiles = bannedFiles;
        }
        public void UpsertDirectory(string path, StorageCacheItem directory)
        {
            if(IsBannedFolder(path)) return;

            DeleteDirectory(path);
            Insert(path, directory);
        }

        public void UpsertFile(IFileItem fileItem)
        {
            if (IsBannedFile(fileItem.Path)) return;

            var fileParentDir = EnsureParents(fileItem.Path);

            fileParentDir!.UpsertFile(fileItem);
            UpsertDirectory(fileParentDir.Path, fileParentDir);
        }

        public StorageCacheItem EnsureParents(string path)
        {
            var parentPath = CleanPath(path.GetUnixParentPath());
            var fileParentDir = GetByDirPath(parentPath);

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
            if (IsBannedFolder(path)) return;

            cacheItem = CleanBadFilesAndFolders(cacheItem);
            var cleanPath = CleanPath(path);
            TryAdd(cleanPath, cacheItem);
        }

        public void DeleteDirectory(string path)
        {
            var cleanPath = CleanPath(path);
            var dir = GetByDirPath(cleanPath);

            if (dir is null) return;

            Remove(cleanPath);
        }

        public void DeleteDirectoryWithChildren(string path)
        {
            var currentDir = GetByDirPath(path);

            if (currentDir is null) return;

            foreach (var directory in currentDir.Directories)
            {
                DeleteDirectoryWithChildren(directory.Path);
            }
            DeleteDirectory(currentDir.Path);
        }

        public StorageCacheItem? GetByDirPath(string path)
        {
            var cleanPath = CleanPath(path);

            if (!TryGetValue(cleanPath, out var item)) return null;

            return item;
        }

        public IFileItem? GetFileByPath(string filePath)
        {
            var parentPath = CleanPath(filePath.GetUnixParentPath());
            TryGetValue(parentPath, out StorageCacheItem? dir);

            if (dir is not null)
            {
                return dir.Files.FirstOrDefault(f => f.Path.Equals(filePath));
            }
            return null;
        }

        public List<IFileItem> GetFileByName(string name)
        {
            return this.SelectMany(c => c.Value.Files)
                .Where(f => f.Name.Equals(name))
                .ToList();
        }

        public void DeleteFile(string path)
        {
            var cleanPath = CleanPath(path);
            var parentPath = cleanPath.GetUnixParentPath();
            var parentDir = GetByDirPath(parentPath);

            if (parentDir is null) return;

            parentDir.DeleteFile(path);
        }

        private static string CleanPath(string path) => path
            .RemoveLeadingAndTrailingSlash();

        private bool IsBannedFolder(string folder) 
        {
            if (folder == StorageConstants.Remote_Path_Root) return false;

            return _bannedFolders.Any(b => b.RemoveLeadingAndTrailingSlash().Contains(folder.RemoveLeadingAndTrailingSlash()));
        }
        private bool IsBannedFile(string fileName) => _bannedFiles.Any(b => b.RemoveLeadingAndTrailingSlash().Contains(fileName.RemoveLeadingAndTrailingSlash()));

        private StorageCacheItem CleanBadFilesAndFolders(StorageCacheItem cacheItem)
        {
            cacheItem.Directories = cacheItem.Directories
                .Where(d => !IsBannedFolder(d.Path))
                .ToList();

            cacheItem.Files = cacheItem.Files
                .Where(f => !IsBannedFile(f.Name))
                .ToList();

            return cacheItem;
        }
    }
}