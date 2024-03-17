
namespace TeensyRom.Core.Storage.Entities
{
    public class GameItem : FileItem, ILaunchableItem, IViewableItem
    {
        public GameItemScreens Screens { get; set; } = new ();
        public List<ViewableItemImage> Images { get; init; } = [];

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
            },
            Images = Images.Select(x => x.Clone()).ToList()
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
