using System.Collections.Generic;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Music
{
    public class MusicDirectory
    {
        public List<DirectoryItem> Directories { get; set; } = new();
        public List<SongItem> Songs { get; set; } = new();

        public void Upsert(SongItem song)
        {
            var songIndex = Songs.IndexOf(song);

            if (songIndex == -1)
            {
                Insert(song);
                return;
            }
            Songs[songIndex] = song;
        }

        public void Upsert(DirectoryItem directory)
        {
            var dirIndex = Directories.IndexOf(directory);

            if (dirIndex == -1)
            {
                Insert(directory);
                return;
            }
            Directories[dirIndex] = directory;
        }

        public void Insert(DirectoryItem directory)
        {
            Directories.Add(directory);
            Directories = Directories.OrderBy(s => s.Name).ToList();
        }

        public void Insert(SongItem song)
        {
            Songs.Add(song);
            Songs = Songs.OrderBy(s => s.Name).ToList();
        }
    }
}
