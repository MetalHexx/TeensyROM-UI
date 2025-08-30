using TeensyRom.Core.ValueObjects;

namespace TeensyRom.Core.Entities.Storage
{
    public class Playlist
    {
        public FilePath Path { get; set; } = new FilePath(string.Empty);
        public List<PlaylistItem> Items { get; set; } = [];
    }
}
