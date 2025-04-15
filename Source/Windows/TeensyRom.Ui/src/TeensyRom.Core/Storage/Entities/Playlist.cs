namespace TeensyRom.Core.Storage.Entities
{
    public class Playlist 
    {
        public string Path { get; set; } = string.Empty;

        public List<PlaylistItem> Items { get; set; } = [];
    }
}
