using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Music
{
    public interface IMusicService
    {
        SongItem EnrichSong(SongItem song, string path);
    }
}