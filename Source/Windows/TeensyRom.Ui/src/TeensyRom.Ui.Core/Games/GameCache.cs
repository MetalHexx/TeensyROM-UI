namespace TeensyRom.Ui.Core.Games
{
    public class GameCache
    {
        public string LoadingScreenImagePath { get; init; } = string.Empty;
        public string ScreenshotImagePath { get; init; } = string.Empty;
        public List<GameCacheItem> LoadingScreenCache { get; init; } = [];
        public List<GameCacheItem> ScreenshotCache { get; init; } = [];
    }
}
