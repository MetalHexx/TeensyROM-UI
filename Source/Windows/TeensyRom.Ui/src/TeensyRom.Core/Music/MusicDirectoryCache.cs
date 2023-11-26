using System.Collections.Generic;
using TeensyRom.Core.Common;

namespace TeensyRom.Core.Music
{
    public class MusicDirectoryCache: Dictionary<string, MusicDirectory>
    {
        public void Insert(string path, MusicDirectory cacheItem)
        {
            path = path
                .RemoveLeadingAndTrailingSlash()
                .ToLower();

            TryAdd(path, cacheItem);
        }
        public MusicDirectory? GetBySong(string path)
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
