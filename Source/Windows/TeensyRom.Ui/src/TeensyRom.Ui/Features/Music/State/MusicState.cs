using DynamicData;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Commands.File.LaunchFile;
using TeensyRom.Core.Common;
using TeensyRom.Core.Music;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Controls;
using TeensyRom.Ui.Features.Common.Models;

namespace TeensyRom.Ui.Features.Music.State
{
    public class MusicState : IMusicState, IDisposable
    {
        public IObservable<DirectoryItem> DirectoryTree => _directoryTree.AsObservable();
        public IObservable<ObservableCollection<StorageItem>> DirectoryContent => _directoryContent.AsObservable();
        public IObservable<bool> DirectoryLoading => _directoryLoading.AsObservable();
        public IObservable<SongItem> CurrentSong => _currentSong.AsObservable();        
        public IObservable<SongMode> CurrentSongMode => _songMode.AsObservable();
        public IObservable<PlayState> CurrentPlayState => _playState.AsObservable();
        public IObservable<TimeSpan> CurrentSongTime => _songTime.CurrentTime;

        private readonly BehaviorSubject<DirectoryItem> _directoryTree = new(new());
        private readonly Subject<ObservableCollection<StorageItem>> _directoryContent = new();
        private readonly Subject<bool> _directoryLoading = new();
        private readonly BehaviorSubject<SongItem> _currentSong = new(null);
        private readonly BehaviorSubject<SongMode> _songMode = new(SongMode.Next);
        private readonly BehaviorSubject<PlayState> _playState = new(PlayState.Paused);     

        private readonly MusicCache _musicCache = new();

        private readonly ISongTimer _songTime;
        private readonly ILaunchFileCommand _launchFileCommand;
        private readonly IGetDirectoryCommand _getDirectoryCommand;
        private readonly IToggleMusicCommand _toggleMusicCommand;
        private readonly IMusicService _musicService;
        private readonly ISettingsService _settingsService;
        private TeensySettings _settings = new();
        private IDisposable? _playingSongSubscription;
        private TimeSpan? _currentTime;
        private IDisposable _currentTimeSubscription;

        public MusicState(ISongTimer songTime, ILaunchFileCommand launchFileCommand, IGetDirectoryCommand getDirectoryCommand, IToggleMusicCommand toggleMusicCommand, IMusicService musicService, ISettingsService settingsService)
        {
            _songTime = songTime;
            _launchFileCommand = launchFileCommand;
            _getDirectoryCommand = getDirectoryCommand;
            _toggleMusicCommand = toggleMusicCommand;
            _musicService = musicService;
            _settingsService = settingsService;
            _settingsService.Settings.Subscribe(settings => OnSettingsChanged(settings));

            //TODO: Clean this up later.
            _currentTimeSubscription = _songTime.CurrentTime.Subscribe(currentTime =>
            {
                _currentTime = currentTime;
            });
        }        

        private void OnSettingsChanged(TeensySettings settings)
        {
            _settings = settings;
            _musicCache.Clear();
            _directoryContent.OnNext(new ObservableCollection<StorageItem>());
            ResetDirectoryTree();
        }

        private void ResetDirectoryTree()
        {
            var dirItem = new DirectoryItem
            {
                Name = "Root",  //The view only maps to the directory enumerable at the top level.  So we create a fake root here.
                Path = _settings.TargetRootPath,
                Directories = new()
                {
                    new DirectoryItem
                    {
                        Name = _settings.GetFileTypePath(TeensyFileType.Sid),
                        Path = _settings.GetFileTypePath(TeensyFileType.Sid)
                    }
                }
            };
            _directoryTree.OnNext(dirItem);
        }

        public void SetSongMode(SongMode songMode) => _songMode.OnNext(songMode);

        public bool LoadSong(SongItem song)
        {
            _songTime.Reset();
            _songTime.Start();

            if (!_launchFileCommand.Execute(song.Path))
            {
                return false;
            }
            _currentSong.OnNext(song);

            _playState.OnNext(PlayState.Playing);

            _playingSongSubscription?.Dispose();

            _playingSongSubscription = Observable.Timer(song.SongLength)
                .Subscribe(_ => PlayNext());

            return true;
        }

        public bool ToggleMusic()
        {
            if (!_toggleMusicCommand.Execute()) return false;

            var playState = GetToggledPlayState();

            if (playState == PlayState.Playing)
            {
                _songTime.Start();
            }
            else
            {
                _songTime.Stop();
            }
            _playState.OnNext(playState);
            return true;
        }

        public void PlayPrevious()
        {
            var cacheItem = _musicCache.GetBySong(_currentSong.Value.Path);

            if(cacheItem is null || _currentTime >= TimeSpan.FromSeconds(3))
            {
                LoadSong(_currentSong.Value);
                return;
            }
            var currentIndex = cacheItem.Songs.IndexOf(_currentSong.Value);

            LoadSong(currentIndex == 0
                ? cacheItem.Songs.Last()
                : cacheItem.Songs[--currentIndex]);

        }

        public void PlayNext()
        {
            var cacheItem = _musicCache.GetBySong(_currentSong.Value.Path);

            if (cacheItem is null)
            {
                LoadSong(_currentSong.Value);
                return;
            }
            var currentIndex = cacheItem.Songs.IndexOf(_currentSong.Value);

            LoadSong(cacheItem.Songs.Count == currentIndex + 1
                ? cacheItem.Songs.First()
                : cacheItem.Songs[++currentIndex]);

        }

        private PlayState GetToggledPlayState() => _playState.Value == PlayState.Playing
                ? PlayState.Paused
                : PlayState.Playing;

        public IObservable<Unit> LoadDirectory(string path)
        {
            return Observable.Start(() =>
            {
                return LoadDirectorySync(path);

            }, RxApp.TaskpoolScheduler)
            .ObserveOn(RxApp.MainThreadScheduler);
        }

        public Unit LoadDirectorySync(string path)
        {
            _directoryLoading.OnNext(true);
            var directoryResult = GetDirectory(path);

            if(directoryResult is null)
            {
                _directoryLoading.OnNext(false);
                return Unit.Default;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                _directoryTree.Value.Insert(directoryResult.Directories);
            });
            _directoryTree.OnNext(_directoryTree.Value);

            var directoryItems = new ObservableCollection<StorageItem>();
            directoryItems.AddRange(directoryResult.Directories);
            directoryItems.AddRange(directoryResult.Songs);

            _directoryContent.OnNext(directoryItems);
            _directoryTree.OnNext(_directoryTree.Value);
            _directoryLoading.OnNext(false);

            return Unit.Default;
        }

        private MusicCacheItem? GetDirectory(string path)
        {
            var cacheItem = _musicCache.GetByDirectory(path);

            if (cacheItem != null) return cacheItem;

            var directoryContent = _getDirectoryCommand.Execute(path, 0, 5000);

            if (directoryContent is null) return null;

            var songs = MapAndOrderSongs(directoryContent);
            var directories = MapAndOrderDirectories(directoryContent);

            cacheItem = new MusicCacheItem
            {
                Directories = directories.ToList(),
                Songs = songs
            };
            _musicCache.Insert(path, cacheItem);

            return cacheItem;
        }

        private static IEnumerable<DirectoryItem> MapAndOrderDirectories(DirectoryContent? directoryContent)
        {
            return directoryContent?.Directories
                .Select(d => new DirectoryItem
                {
                    Name = d.Name,
                    Path = d.Path
                })
                .OrderBy(d => d.Name)
                .ToList() ?? new List<DirectoryItem>();
        }

        private List<SongItem> MapAndOrderSongs(DirectoryContent? directoryContent)
        {
            return directoryContent?.Files
                .Select(file => new SongItem
                {
                    Name = file.Name,
                    Path = file.Path,
                    Size = file.Size
                })
                .Select(song =>
                {
                    var defaultSidPath = _settings.GetFileTypePath(TeensyFileType.Sid);
                    var trimmedPath = song.Path.Replace($"{defaultSidPath}/hvsc", "");
                    return _musicService.EnrichSong(song, trimmedPath);
                })
                .OrderBy(song => song.Name)
                .ToList() ?? new List<SongItem>();
        }

        public void Dispose()
        {
            _currentTimeSubscription?.Dispose();
            _playingSongSubscription?.Dispose();
        }
    }
}
