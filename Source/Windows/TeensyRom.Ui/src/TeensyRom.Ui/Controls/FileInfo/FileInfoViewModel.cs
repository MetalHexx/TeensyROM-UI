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
using TeensyRom.Ui.Features.Common.State.Player;

namespace TeensyRom.Ui.Controls.FileInfo
{
    public class FileInfoViewModel : ReactiveObject
    {
        [ObservableAsProperty] public string? Title { get; private set; }
        [ObservableAsProperty] public string? LoadingScreenPath { get; }
        [ObservableAsProperty] public string? ScreenshotPath { get; }
        [ObservableAsProperty] public ImageSource? CroppedLoadingScreen { get; }
        [ObservableAsProperty] public ImageSource? CroppedScreenshot { get; }
        [ObservableAsProperty] public ILaunchableItem? SelectedFile { get; }

        public FileInfoViewModel(IPlayerContext context, IGameMetadataService gameMetadata)
        {
            var selectedFile = context.SelectedFile;

            selectedFile
                .Where(type => type is not GameItem)
                .Select(file => file?.Title ?? string.Empty)
                .ToPropertyEx(this, x => x.Title);

            context.SelectedFile.ToPropertyEx(this, x => x.SelectedFile);

            var selectedGame = selectedFile.OfType<GameItem>();

            //selectedGame
            //    .Select(file => file.Title[..file.Title.LastIndexOf('.')])
            //    .ToPropertyEx(this, x => x.Title);

            selectedGame.Subscribe(gameMetadata.GetGameScreens);

            selectedGame
                .Select(game => game.Screens.LoadingScreenLocalPath)
                .ToPropertyEx(this, x => x.LoadingScreenPath);

            selectedGame
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
