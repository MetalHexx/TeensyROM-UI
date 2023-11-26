using System.Collections.Generic;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Music
{
    public class MusicDirectory
    {
        public List<DirectoryItem> Directories { get; set; } = new();
        public List<SongItem> Songs { get; set; } = new();
    }
}
