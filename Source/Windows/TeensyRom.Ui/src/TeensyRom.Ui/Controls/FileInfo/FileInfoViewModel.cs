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
using System.Collections;
using System.Collections.ObjectModel;
using TeensyRom.Core.Common;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;

namespace TeensyRom.Ui.Controls.FileInfo
{
    public class ImageAndMetadata
    {
        public ImageSource? Image { get; set; }
        public string? MetadataSource { get; set; }
    }
    public class FileInfoViewModel : ReactiveObject
    {
        [ObservableAsProperty] public ObservableCollection<ImageAndMetadata> ImageSources { get; } = [];
        [ObservableAsProperty] public string? Title { get; private set; }        
        [ObservableAsProperty] public string? LoadingScreenPath { get; }
        [ObservableAsProperty] public string? ScreenshotPath { get; }
        [ObservableAsProperty] public ImageSource? CroppedLoadingScreen { get; }
        [ObservableAsProperty] public ImageSource? CroppedScreenshot { get; }
        [ObservableAsProperty] public ILaunchableItem? SelectedFile { get; }

        public FileInfoViewModel(IPlayerContext context, IGameMetadataService gameMetadata)
        {
            var selectedFile = context.SelectedFile.SubscribeOn(RxApp.MainThreadScheduler);

            selectedFile
                .Where(type => type is not GameItem)
                .Select(file => file?.Title ?? string.Empty)
                .ToPropertyEx(this, x => x.Title);

            context.SelectedFile.ToPropertyEx(this, x => x.SelectedFile);

            selectedFile
                .OfType<IViewableItem>()
                .Select(item => item.Images
                    .Where(image => !string.IsNullOrEmpty(image.LocalPath))
                    .Select(image => 
                    {
                        return item is GameItem
                            ? new ImageAndMetadata { Image = CreateImageSource(image.LocalPath), MetadataSource = image.Source }
                            : new ImageAndMetadata { Image = CreateImageSource(image.LocalPath) };
                    })
                    .ToList())
                .Select(list => new ObservableCollection<ImageAndMetadata>(list))
                .ToPropertyEx(this, x => x.ImageSources);

            var selectedGame = selectedFile.OfType<GameItem>();

            selectedGame.Subscribe(gameMetadata.GetGameScreens);
        }

        private ImageSource? CreateImageSource(string imagePath, int fromX = 0, int fromY = 0)
        {
            if (string.IsNullOrEmpty(imagePath)) return null;

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

        private ImageSource? CreateImageSource(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return null;

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                return bitmap;
            }
            catch
            {
                return null;
            }
        }
    }
}
