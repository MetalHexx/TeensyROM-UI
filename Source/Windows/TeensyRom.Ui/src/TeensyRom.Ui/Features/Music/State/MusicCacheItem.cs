using System.Collections.Generic;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Ui.Features.Music.State
{
    public class MusicCacheItem
    {
        public List<DirectoryItem> Directories { get; set; } = new();
        public List<SongItem> Songs { get; set; } = new();
    }
}
