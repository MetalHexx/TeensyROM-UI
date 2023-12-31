using System.Collections.Generic;
using System.IO;
using System.Reactive.Concurrency;
using TeensyRom.Core.Common;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Music
{
    public class FileDirectoryCache : Dictionary<string, FileDirectory>
    {
        public void UpsertDirectory(string path, FileDirectory directory)
        {
            DeleteDirectory(path);
            InsertDirectory(path, directory);
        }
        
        public void UpsertFile(FileItem fileItem)
        {
            var path = fileItem.Path.GetParentDirectory();
            var fileParentDir = GetByDirectory(path);

            if (fileParentDir is null) return;

            fileParentDir!.UpsertFile(fileItem);
            UpsertDirectory(path, fileParentDir);
        }

        private void InsertDirectory(string path, FileDirectory cacheItem)
        {
            var cleanPath = CleanPath(path);
            TryAdd(cleanPath, cacheItem);
        }

        public void DeleteDirectory(string path)
        {
            var cleanPath = CleanPath(path);
            var dir = GetByDirectory(cleanPath);

            if (dir is null) return;

            Remove(cleanPath);
        }

        public void DeleteDirectoryTree(string path)
        {
            var currentDir = GetByDirectory(path);

            if (currentDir is null) return;

            foreach (var directory in currentDir.Directories)
            {
                DeleteDirectoryTree(directory.Path);
            }
            DeleteDirectory(currentDir.Path);
        }

        public FileDirectory? GetByDirectory(string path)
        {
            var cleanPath = CleanPath(path);

            if (!TryGetValue(cleanPath, out var item)) return null;

            return item;
        }

        private static string CleanPath(string path) => path
            .RemoveLeadingAndTrailingSlash()
            .ToLower();
    }
}