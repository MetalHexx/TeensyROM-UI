using System.Collections.Generic;
using TeensyRom.Core.Common;

namespace TeensyRom.Ui.Features.Music.State
{
    public class MusicCache: Dictionary<string, MusicCacheItem>
    {
        public void Insert(string path, MusicCacheItem cacheItem)
        {
            path = path
                .RemoveLeadingAndTrailingSlash()
                .ToLower();

            TryAdd(path, cacheItem);
        }
        public MusicCacheItem? GetBySong(string path)
        {
            var currentSongDirectory = path
                .GetParentDirectory()
                .RemoveLeadingAndTrailingSlash()
                .ToLower();

            if (!TryGetValue(currentSongDirectory, out var item)) return null;

            return item;
        }

        public MusicCacheItem? GetByDirectory(string path)
        {
            var currentSongDirectory = path
                .RemoveLeadingAndTrailingSlash()
                .ToLower();

            if (!TryGetValue(currentSongDirectory, out var item)) return null;

            return item;
        }
    }
}
