using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive.Linq;
using System.Diagnostics;
using System.Windows;
using TeensyRom.Ui.Core.Games;
using TeensyRom.Ui.Core.Storage.Entities;
using System.Collections;
using System.Collections.ObjectModel;
using TeensyRom.Ui.Core.Common;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using TeensyRom.Ui.Features.Discover.State.Player;

namespace TeensyRom.Ui.Controls.FileInfo
{
    public class ImageAndMetadata
    {
        public ImageSource? Image { get; set; }
        public string? MetadataSource { get; set; }
    }
    public class FileInfoViewModel : ReactiveObject
    {
        [ObservableAsProperty] public ObservableCollection<ImageAndMetadata>? ImageSources { get; } = [];
        [ObservableAsProperty] public string? Title { get; private set; }        
        [ObservableAsProperty] public string? LoadingScreenPath { get; }
        [ObservableAsProperty] public string? ScreenshotPath { get; }
        [ObservableAsProperty] public ImageSource? CroppedLoadingScreen { get; }
        [ObservableAsProperty] public ImageSource? CroppedScreenshot { get; }
        [ObservableAsProperty] public ILaunchableItem? SelectedFile { get; }

        public FileInfoViewModel(IPlayerContext context, IGameMetadataService gameMetadata)
        {
            var selectedFile = context.SelectedFile.ObserveOn(RxApp.MainThreadScheduler);

            selectedFile
                .Where(type => type is not GameItem)
                .Select(file => file?.Title ?? string.Empty)
                .ToPropertyEx(this, x => x.Title);

            selectedFile.ToPropertyEx(this, x => x.SelectedFile);

            selectedFile
                .OfType<ImageItem>()
                .Subscribe(_ => ImageSources?.Clear());

            selectedFile
                .OfType<IViewableItem>()
                .Select(item => item.Images
                    .Where(image => !string.IsNullOrEmpty(image.Path))
                    .Select(image => 
                    {
                        return item is GameItem
                            ? new ImageAndMetadata { Image = CreateGameImageSource(image.Path), MetadataSource = image.Source }
                            : new ImageAndMetadata { Image = CreateImageSource(image.Path), MetadataSource = image.Source };
                    })
                    .ToList())
                .Select(list => new ObservableCollection<ImageAndMetadata>(list))
                .ToPropertyEx(this, x => x.ImageSources);

            var selectedGame = selectedFile.OfType<GameItem>();
        }

        /// <summary>
        /// Creates an image source for a game item.  If the file is a screenshot, it'll crop the image
        /// to remove the border and bring the image to 320x200.
        /// </summary>
        /// <param name="imagePath"></param>
        /// <returns></returns>
        private ImageSource? CreateGameImageSource(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath)) return null;

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                if (bitmap.PixelWidth == 0 || bitmap.PixelHeight == 0) return null;

                int targetWidth = 320;
                int targetHeight = 200;

                int fromX = Math.Max(0, (bitmap.PixelWidth - targetWidth) / 2);
                int fromY = Math.Max(0, (bitmap.PixelHeight - targetHeight) / 2);

                targetWidth = Math.Min(targetWidth, bitmap.PixelWidth);
                targetHeight = Math.Min(targetHeight, bitmap.PixelHeight);

                var rect = new Int32Rect(fromX, fromY, targetWidth, targetHeight);
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
