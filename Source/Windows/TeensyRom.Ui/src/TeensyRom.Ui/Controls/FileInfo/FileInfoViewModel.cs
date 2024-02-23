using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive.Linq;
using System.Diagnostics;
using System.Windows;
using TeensyRom.Core.Games;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Features.Games.State;

namespace TeensyRom.Ui.Controls.FileInfo
{
    public class FileInfoViewModel : ReactiveObject
    {
        [ObservableAsProperty] public string? Title { get; private set; }
        [ObservableAsProperty] public string? LoadingScreenPath { get; }
        [ObservableAsProperty] public string? ScreenshotPath { get; }
        [ObservableAsProperty] public ImageSource? CroppedLoadingScreen { get; }
        [ObservableAsProperty] public ImageSource? CroppedScreenshot { get; }

        public FileInfoViewModel(IPlayerContext context, IGameMetadataService gameMetadata)
        {
            context.SelectedFile
                .Where(game => game != null)
                .OfType<GameItem>()
                .Do(gameMetadata.GetGameScreens)
                .Select(game => game.Name[..game.Name.LastIndexOf('.')])
                .ToPropertyEx(this, x => x.Title);

            context.SelectedFile
                .Where(game => game != null)
                .OfType<GameItem>()
                .Select(game => game.Screens.LoadingScreenLocalPath)
                .ToPropertyEx(this, x => x.LoadingScreenPath);

            context.SelectedFile
                .Where(game => game != null)
                .OfType<GameItem>()
                .Select(game => game.Screens.ScreenshotLocalPath)
                .ToPropertyEx(this, x => x.ScreenshotPath);

            this.WhenAnyValue(x => x.LoadingScreenPath)
                .Select(path => CreateImageSource(path!))
                .ToPropertyEx(this, x => x.CroppedLoadingScreen);

            this.WhenAnyValue(x => x.ScreenshotPath)
                .Select(path => CreateImageSource(path!, 32, 36))
                .ToPropertyEx(this, x => x.CroppedScreenshot);
        }

        private ImageSource? CreateImageSource(string imagePath, int fromX = 0, int fromY = 0)
        {
            if (string.IsNullOrEmpty(imagePath))
                return null;

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                var rect = new Int32Rect(fromX, fromY, 320, 200);
                var croppedBitmap = new CroppedBitmap(bitmap, rect);
                return croppedBitmap;
            }
            catch
            {
                return null;
            }
        }
    }
}
