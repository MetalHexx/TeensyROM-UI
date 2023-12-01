using System.Collections.Generic;
using System.IO;
using System.Reactive.Concurrency;
using TeensyRom.Core.Common;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Music
{
    public class MusicDirectoryCache: Dictionary<string, MusicDirectory>
    {
        public void UpsertDirectory(string path, MusicDirectory directory)
        {
            DeleteDirectory(path);
            InsertDirectory(path, directory);
        }
        
        public void UpsertSong(SongItem song)
        {
            var path = song.Path.GetParentDirectory();
            var songParentDir = GetByDirectory(path);

            if (songParentDir is null) return;

            songParentDir!.UpsertSong(song);
            UpsertDirectory(path, songParentDir);
        }

        private void InsertDirectory(string path, MusicDirectory cacheItem)
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

        public MusicDirectory? GetByDirectory(string path)
        {
            var cleanPath = CleanPath(path);

            if (!TryGetValue(cleanPath, out var item)) return null;

            return item;
        }

        private string CleanPath(string path) => path
            .RemoveLeadingAndTrailingSlash()
            .ToLower();
    }
}
