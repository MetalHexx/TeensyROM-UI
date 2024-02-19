namespace TeensyRom.Core.Storage.Entities
{
    public class GameItem : FileItem, ILaunchableItem
    {
        public GameItemScreens Screens { get; set; } = new ();
        public override GameItem Clone() => new ()
        {
            Name = Name,
            Path = Path,
            Size = Size,
            IsFavorite = IsFavorite,
            Screens = new GameItemScreens
            {
                LoadingScreenLocalPath = Screens.LoadingScreenLocalPath,
                LoadingScreenRemotePath = Screens.LoadingScreenRemotePath,
                ScreenshotLocalPath = Screens.ScreenshotLocalPath,
                ScreenshotRemotePath = Screens.ScreenshotRemotePath                
            }
        };
    }

    public class GameItemScreens
    {
        public string LoadingScreenLocalPath { get; set; } = string.Empty;
        public string LoadingScreenRemotePath { get; set; } = string.Empty;
        public string ScreenshotLocalPath { get; set; } = string.Empty;
        public string ScreenshotRemotePath { get; set; } = string.Empty;
    }
}
