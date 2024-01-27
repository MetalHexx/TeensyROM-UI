using DynamicData;
using MediatR;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Commands.File.LaunchFile;
using TeensyRom.Core.Common;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Core.Storage.Services;
using TeensyRom.Ui.Controls;
using TeensyRom.Ui.Controls.DirectoryTree;
using TeensyRom.Ui.Features.Global;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Services;

namespace TeensyRom.Ui.Features.Music.State
{
    public class MusicState : IMusicState, IDisposable
    {
        public IObservable<DirectoryNodeViewModel> DirectoryTree => _directoryTree.AsObservable();
        public IObservable<ObservableCollection<StorageItem>> DirectoryContent => _directoryContent.AsObservable();
        public IObservable<SongItem> CurrentSong => _currentSong.AsObservable();        
        public IObservable<SongMode> CurrentSongMode => _songMode.AsObservable();
        public IObservable<PlayState> CurrentPlayState => _playState.AsObservable();
        public IObservable<TimeSpan> CurrentSongTime => _songTime.CurrentTime;

        private readonly BehaviorSubject<DirectoryNodeViewModel> _directoryTree = new(new());
        private readonly Subject<ObservableCollection<StorageItem>> _directoryContent = new();
        private readonly BehaviorSubject<StorageCacheItem?> _currentDirectory = new(null);
        private readonly BehaviorSubject<SongItem> _currentSong = new(null);
        private readonly BehaviorSubject<SongMode> _songMode = new(SongMode.Next);
        private readonly BehaviorSubject<PlayState> _playState = new(PlayState.Paused);     

        private readonly ISongTimer _songTime;
        private readonly IMediator _mediator;
        private readonly ICachedStorageService _musicService;
        private readonly ISettingsService _settingsService;
        private readonly ILaunchHistory _launchHistory;
        private readonly ISnackbarService _alert;
        private TeensySettings _settings = new();
        private TimeSpan? _currentTime;
        private IDisposable? _playingSongSubscription;
        private IDisposable _programLaunchedSubscription;
        private IDisposable _settingsSubscription;

        private IDisposable _currentTimeSubscription;

        public MusicState(ISongTimer songTime, IMediator mediator, ICachedStorageService musicService, ISettingsService settingsService, ILaunchHistory launchHistory, ISnackbarService alert, IGlobalState globalState, ISerialStateContext serialContext, INavigationService nav)
        {
            _songTime = songTime;
            _mediator = mediator;
            _musicService = musicService;
            _settingsService = settingsService;
            _launchHistory = launchHistory;
            _alert = alert;
            _settingsService.Settings.Subscribe(OnSettingsChanged);

            _settingsSubscription = _settingsService.Settings
                .Do(settings => _settings = settings)
                .CombineLatest(serialContext.CurrentState, nav.SelectedNavigationView, (settings, serial, navView) => (settings, serial, navView))
                .Where(state => state.serial is SerialConnectedState)
                .Where(state => state.navView?.Type == NavigationLocation.Music)
                .Select(state => (path: state.settings.GetLibraryPath(TeensyLibraryType.Music), state.settings.TargetType))
                .DistinctUntilChanged()
                .Select(storage => storage.path)
                .Do(_ => ResetDirectoryTree())
                .Subscribe(async path => await LoadDirectory(path));

            _currentTimeSubscription = _songTime.CurrentTime.Subscribe(currentTime =>
            {
                _currentTime = currentTime;
            });

            musicService.DirectoryUpdated
                .Where(path => path.Equals(_currentDirectory.Value?.Path))
                .Subscribe(async _ => await RefreshDirectory(bustCache: false));

            _programLaunchedSubscription = globalState.FileViewLaunched
                .WithLatestFrom(_playState, (file, playState) => playState)
                .Where(playState => playState == PlayState.Playing)
                .Subscribe(async _ => await ToggleMusic());
        }        

        private void OnSettingsChanged(TeensySettings settings)
        {
            _settings = settings;
            _directoryContent.OnNext(new ObservableCollection<StorageItem>());
            ResetDirectoryTree();
        }

        private void ResetDirectoryTree()
        {
            var musicLibraryPath = _settings.GetLibraryPath(TeensyLibraryType.Music);

            var dirItem = new DirectoryNodeViewModel
            {
                Name = "Fake Root",  //TODO: Fake root required since UI view binds to enumerable -- design could use improvement
                Path = "Fake Root",
                Directories = 
                [
                    new DirectoryNodeViewModel
                    {
                        Name = musicLibraryPath,
                        Path = musicLibraryPath,
                        Directories = []
                    }
                ]
            };
            _directoryTree.OnNext(dirItem);
        }

        public Unit ToggleShuffleMode() 
        {
            if(_songMode.Value == SongMode.Shuffle)
            {
                _songMode.OnNext(SongMode.Next);
                return Unit.Default;
            }
            _songMode.OnNext(SongMode.Shuffle);
            return Unit.Default;
        }

        public async Task<bool> LoadSong(SongItem song, bool clearHistory = true)
        {
            if (clearHistory) 
            {
                _launchHistory.Clear();
                _songMode.OnNext(SongMode.Next);
            }

            _songTime.StartNewTimer(song.SongLength);
            
            var result = await _mediator.Send(new LaunchFileCommand { Path = song.Path }); //TODO: When TR fails on a SID, the next song command "fails", but the song still plays.  So I'll ignore the false return value for now.

            Application.Current.Dispatcher.Invoke(() =>
            {
                song.IsSelected = true;

                var shouldUpdateCurrent = _currentSong.Value is not null 
                    && song.Path.Equals(_currentSong.Value.Path) == false;

                if (shouldUpdateCurrent)
                {
                    _currentSong.Value!.IsSelected = false;
                }
            });

            _currentSong.OnNext(song);
            _playState.OnNext(PlayState.Playing);
            

            _playingSongSubscription?.Dispose();
            _playingSongSubscription = _songTime.SongComplete.Subscribe(async _ => 
            {
                if (_songMode.Value == SongMode.Shuffle)
                {
                    await PlayRandom();
                    return;
                }
                await PlayNext();
            });

            if (result.LaunchResult is LaunchFileResultType.SidError)
            {
                _alert.Enqueue("Incompatible SID detected (see logs).  Attempting to play the next track.");
                await PlayNext();
                return false;
            }

            return true;
        }

        public async Task<bool> SaveFavorite(SongItem song)
        {
            var favSong = await _musicService.SaveFavorite(song);
            var songParentDir = favSong?.Path.GetUnixParentPath();

            if(songParentDir is null) return false;
            
            var directoryResult = await _musicService.GetDirectory(songParentDir);

            if (directoryResult is null) return false;

            Application.Current.Dispatcher.Invoke(() =>
            {
                _directoryTree.Value.Insert(directoryResult.Directories);
            });

            _directoryTree.OnNext(_directoryTree.Value);

            return true;
        }

        public async Task ToggleMusic()
        {
            var result = await _mediator.Send(new ToggleMusicCommand());
            if (!result.IsSuccess) return;
            TogglePlayState();            
        }

        private PlayState TogglePlayState()
        {
            var playState = GetToggledPlayState();

            if (playState == PlayState.Playing)
            {
                _songTime.ResumeTimer();
            }
            else
            {
                _songTime.PauseTimer();
            }
            _playState.OnNext(playState);
            return playState;
        }

        public Task PlayPrevious()
        {
            return _songMode.Value switch
            {
                SongMode.Next => PlayPreviousInDirectory(),
                SongMode.Shuffle =>  PlayPreviousShuffle(),
                _ => throw new NotImplementedException()
            };
        }

        private async Task PlayPreviousInDirectory()
        {
            var parentPath = _currentSong.Value.Path.GetUnixParentPath();
            var directoryResult = await _musicService.GetDirectory(parentPath);

            if (directoryResult is null || _currentTime >= TimeSpan.FromSeconds(3))
            {
                await LoadSong(_currentSong.Value);
                return;
            }
            var songIndex = directoryResult.Files.IndexOf(_currentSong.Value);

            var songToLoad = songIndex == 0
                ? directoryResult.Files.Last()
                : directoryResult.Files[--songIndex];

            await LoadSong((songToLoad as SongItem)!);
        }

        public async Task PlayPreviousShuffle()
        {
            var song = _launchHistory.GetPrevious(TeensyFileType.Sid);

            if (song is SongItem songItem)
            {
                await LoadDirectory(songItem.Path.GetUnixParentPath());
                await LoadSong(songItem, clearHistory: false);
                return;
            }
            await LoadSong(_currentSong.Value, clearHistory: false);
            return;
        }

        public Task PlayNext()
        {
            return _songMode.Value switch
            {
                SongMode.Next => PlayNextInDirectory(),
                SongMode.Shuffle => PlayNextShuffle(),
                _ => throw new NotImplementedException()
            };
        }

        private async Task PlayNextInDirectory()
        {
            if(_currentSong.Value == null)
            {
                await PlayRandom();
                return;
            }
            var parentPath = _currentSong.Value.Path.GetUnixParentPath();
            var directoryResult = await _musicService.GetDirectory(parentPath);

            if (directoryResult is null)
            {
                await LoadSong(_currentSong.Value);
                return;
            }
            var currentIndex = directoryResult.Files.IndexOf(_currentSong.Value);

            var songToLoad = directoryResult.Files.Count == currentIndex + 1
                ? directoryResult.Files.First()
                : directoryResult.Files[++currentIndex];

            if (songToLoad is SongItem songItem)
            {
                await LoadSong(songItem);
                return;
            }
            await LoadSong(_currentSong.Value);
        }

        private async Task PlayNextShuffle()
        {
            var song = _launchHistory.GetNext(TeensyFileType.Sid);

            if (song is SongItem songItem)
            {
                await LoadDirectory(songItem.Path.GetUnixParentPath());
                await LoadSong(songItem, clearHistory: false);
                return;
            }
            var newSong = await PlayRandom();
            
            if (newSong is null) return;
            
            return;
        }

        public async Task LoadDirectory(string path)
        {
            var directoryResult = await _musicService.GetDirectory(path);

            if (directoryResult is null)
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                _directoryTree.Value.Insert(directoryResult.Directories);
                _directoryTree.Value.SelectDirectory(path);
            });

            _directoryTree.OnNext(_directoryTree.Value);

            var directoryItems = new ObservableCollection<StorageItem>();
            directoryItems.AddRange(directoryResult.Directories);
            directoryItems.AddRange(directoryResult.Files);

            _directoryContent.OnNext(directoryItems);
            _currentDirectory.OnNext(directoryResult);
            _directoryTree.OnNext(_directoryTree.Value);

            return;
        }

        private PlayState GetToggledPlayState() => _playState.Value == PlayState.Playing
                ? PlayState.Paused
                : PlayState.Playing;

        public async Task RefreshDirectory(bool bustCache = true)
        {
            if (_currentDirectory.Value is null) return;

            if(bustCache) _musicService.ClearCache(_currentDirectory.Value.Path);

            await LoadDirectory(_currentDirectory.Value.Path);
        }

        public async Task DeleteFile(FileItem file)
        {
            await _musicService.DeleteFile(file, _settings.TargetType);
            await RefreshDirectory(bustCache: false);
        }

        public async Task<SongItem?> PlayRandom() 
        {
            var song = _musicService.GetRandomFile(TeensyFileType.Sid);

            if(song is SongItem songItem)
            {
                await LoadDirectory(songItem.Path.GetUnixParentPath());
                await LoadSong(songItem, clearHistory: false);

                if(_songMode.Value != SongMode.Shuffle) ToggleShuffleMode();

                _launchHistory.Add(songItem!);

                return songItem;
            }
            _alert.Enqueue("Random search requires visiting at least one directory with music in it first.  Try the cache button next to the dice for best results.");
            return null;
        }

        public Unit SearchMusic(string searchText)
        {
            var searchResult = _musicService.SearchMusic(searchText);

            if (searchResult is null) return Unit.Default;

            _directoryContent.OnNext(new ObservableCollection<StorageItem>(searchResult));
            return Unit.Default;
        }

        public Task ClearSearch()
        {
            return LoadDirectory(_currentDirectory.Value?.Path ?? "/");
        }

        public Task CacheAll() => _musicService.CacheAll();

        public void Dispose()
        {
            _settingsSubscription?.Dispose();
            _currentTimeSubscription?.Dispose();
            _playingSongSubscription?.Dispose();
            _programLaunchedSubscription?.Dispose();
        }
    }
}
