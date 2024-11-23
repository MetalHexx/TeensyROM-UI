namespace TeensyRom.Ui.Core.Games
{
    public class GameCacheItem
    {
        public string Name { get; set; } = string.Empty;
        public string RemotePath { get; init; } = string.Empty;
        public string LocalPath { get; init; } = string.Empty;
        public GameMetadataType Type { get; set; }
    }
}
