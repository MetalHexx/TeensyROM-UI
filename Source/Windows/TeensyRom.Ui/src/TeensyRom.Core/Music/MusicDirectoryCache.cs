using System.Collections.Generic;
using TeensyRom.Core.Common;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Music
{
    public class MusicDirectoryCache: Dictionary<string, MusicDirectory>
    {
        public void Upsert(string path, MusicDirectory directory)
        {
            Delete(path);
            Insert(path, directory);
        }
        public void Insert(string path, MusicDirectory cacheItem)
        {
            path = path
                .RemoveLeadingAndTrailingSlash()
                .ToLower();

            TryAdd(path, cacheItem);
        }

        public void Delete(string path)
        {
            var dir = GetByDirectory(path);

            if (dir is null) return;

            Remove(path);
        }
        public void Upsert(SongItem song)
        {
            var songParentDir = GetParentDirectory(song.Path);
            songParentDir!.Upsert(song);
            Upsert(song.Path.GetParentDirectory(), songParentDir);
        }
        public MusicDirectory? GetParentDirectory(string path)
        {
            var currentSongDirectory = path
                .GetParentDirectory()
                .RemoveLeadingAndTrailingSlash()
                .ToLower();

            if (!TryGetValue(currentSongDirectory, out var item)) return null;

            return item;
        }

        public MusicDirectory? GetByDirectory(string path)
        {
            var currentSongDirectory = path
                .RemoveLeadingAndTrailingSlash()
                .ToLower();

            if (!TryGetValue(currentSongDirectory, out var item)) return null;

            return item;
        }
    }
}
