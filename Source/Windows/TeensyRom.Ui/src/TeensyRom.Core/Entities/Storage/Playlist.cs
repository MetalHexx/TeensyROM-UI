namespace TeensyRom.Core.Entities.Storage
{
    public class Playlist
    {
        public string Path { get; set; } = string.Empty;
        public List<PlaylistItem> Items { get; set; } = [];
    }
}
