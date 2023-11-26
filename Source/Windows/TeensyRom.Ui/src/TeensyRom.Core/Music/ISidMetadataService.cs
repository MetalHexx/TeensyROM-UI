using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Music
{
    public interface ISidMetadataService
    {
        SongItem EnrichSong(SongItem song, string path);
    }
}