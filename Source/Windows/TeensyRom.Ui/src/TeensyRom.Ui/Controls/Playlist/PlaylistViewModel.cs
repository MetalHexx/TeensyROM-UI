using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage;
using TeensyRom.Core.ValueObjects;
using TeensyRom.Ui.Features.Discover.State.Player;
using TeensyRom.Ui.Helpers;
using TeensyRom.Ui.Services.Process;

namespace TeensyRom.Ui.Controls.Playlist
{
    public class PlaylistItemViewModel : ReactiveObject
    {
        [Reactive] public string Name { get; set; } = string.Empty;
        [Reactive] public DirectoryPath Path { get; set; } = new DirectoryPath(string.Empty);
        [Reactive] public bool IsSelected { get; set; }
    }

    public class PlaylistViewModel : ReactiveObject
    {
        private readonly ObservableAsPropertyHelper<bool> _canCreatePlaylist;
        public bool CanCreatePlaylist => _canCreatePlaylist.Value;
        [Reactive] public ObservableCollection<PlaylistItemViewModel> PlaylistItems { get; set; } = [];
        [Reactive] public PlaylistItemViewModel NewPlaylist { get; set; } = new();
        [Reactive] public LaunchableItem FileToAdd { get; private set; } = null!;
        public ReactiveCommand<PlaylistItemViewModel, Unit> CreatePlaylistCommand { get; set; }
        public ReactiveCommand<Unit, Unit> SaveCommand { get; set; }

        private readonly ICachedStorageService _cache;
        private readonly IAlertService _alert;
        private readonly ISettingsService _settingService;
        private readonly ICrossProcessService _crossProcess;
        private const string Playlist_Path = "/playlists";

        public PlaylistViewModel(ICachedStorageService cache, IAlertService alert, ISettingsService settingService, IPlayerContext playerContext, ICrossProcessService crossProcess)
        {
            _cache = cache;
            _alert = alert;
            _settingService = settingService;
            _crossProcess = crossProcess;
            SaveCommand = ReactiveCommand.CreateFromTask(HandleSave);

            _canCreatePlaylist = this.WhenAnyValue(vm => vm.NewPlaylist.Name)
                .Select(name => !string.IsNullOrWhiteSpace(name) && name.IsSafeUnixDirectoryName())
                .ToProperty(this, vm => vm.CanCreatePlaylist);

            CreatePlaylistCommand = ReactiveCommand.Create<PlaylistItemViewModel>(
                HandleCreatePlaylist,
                this.WhenAnyValue(vm => vm.CanCreatePlaylist)
            );
        }

        public async Task InitializeAsync(LaunchableItem fileToAdd)
        {
            FileToAdd = fileToAdd;
            NewPlaylist = new();
            await LoadPlaylists();
        }

        private void HandleCreatePlaylist(PlaylistItemViewModel item)
        {
            var existingItem = PlaylistItems.FirstOrDefault(p => p.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase));

            if (existingItem is not null)
            {
                existingItem.IsSelected = true;
            }
            else 
            {
                item.IsSelected = true;
                item.Path = new DirectoryPath($"{StorageHelper.Playlist_Path}{item.Name}");
                PlaylistItems.Add(item);
            }
            NewPlaylist = new();
        }

        private async Task HandleSave()
        {
            var filesToCopy = PlaylistItems
                .Where(i => i.IsSelected)
                .Select(i => new CopyFileItem(FileToAdd, i.Path))
                .ToList();

            if (filesToCopy.Count == 0)
            {
                _alert.Publish("No playlists selected.");
                return;
            }
            await _crossProcess.CopyFiles(filesToCopy);

            CloseDialog();
        }

        private async Task LoadPlaylists()
        {
            var playlistDirectory = await _cache.GetDirectory(new DirectoryPath(Playlist_Path));
            
            if (playlistDirectory is null || playlistDirectory?.Directories.Count == 0)
            {
                return;
            }

            var favoritesPath = StorageHelper.Favorites_Path.RemoveLeadingAndTrailingSlash();

            var items = playlistDirectory!.Directories
                .Where(d => !d.Path.Equals(favoritesPath))
                .Select(d => new PlaylistItemViewModel
                {
                    Name = d.Name,
                    Path = d.Path,
                });

            var settings = _settingService.GetSettings();

            var favorites = settings
                .GetAllFavoritePaths()
                .Select(p => 
                {
                    var favoritesPath = StorageHelper.GetFavoritePath(FileToAdd.FileType);

                    return new PlaylistItemViewModel
                    {
                        Name = GetFavoritesPlaylistNames(p.Value),
                        Path = p,
                        IsSelected = favoritesPath.Equals(p)
                    };
                });

            PlaylistItems.Clear();
            PlaylistItems.AddRange(favorites);
            PlaylistItems.AddRange(items);
        }

        private static string GetFavoritesPlaylistNames(string path)
        {
            return string.Join(" ", path.Split('/')
                .Skip(1)
                .Where(s => s.ToLower() != "playlists")
                .Select(s => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.Replace("-", " "))))
                .Replace("Favorites", "Favorite");
        }

        private void CloseDialog() 
        {
            MaterialDesignThemes.Wpf.DialogHost.Close(UiConstants.RootDialog);
        }
    }
}