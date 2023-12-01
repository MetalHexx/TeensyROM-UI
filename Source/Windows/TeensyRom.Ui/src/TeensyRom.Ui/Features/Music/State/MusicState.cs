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
        private readonly BehaviorSubject<MusicDirectory?> _currentDirectory = new(null);
        private readonly Subject<bool> _directoryLoading = new();
        private readonly BehaviorSubject<SongItem> _currentSong = new(null);
        private readonly BehaviorSubject<SongMode> _songMode = new(SongMode.Next);
        private readonly BehaviorSubject<PlayState> _playState = new(PlayState.Paused);     

        private readonly ISongTimer _songTime;
        private readonly ILaunchFileCommand _launchFileCommand;
        private readonly IToggleMusicCommand _toggleMusicCommand;
        private readonly IMusicStorageService _musicService;
        private readonly ISettingsService _settingsService;
        private TeensySettings _settings = new();
        private IDisposable? _playingSongSubscription;
        private TimeSpan? _currentTime;
        private IDisposable _currentTimeSubscription;

        public MusicState(ISongTimer songTime, ILaunchFileCommand launchFileCommand, IToggleMusicCommand toggleMusicCommand, IMusicStorageService musicService, ISettingsService settingsService)
        {
            _songTime = songTime;
            _launchFileCommand = launchFileCommand;
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

            _playingSongSubscription = Observable.Timer(song.SongLength)
                .Subscribe(_ => PlayNext());

            return true;
        }

        public bool SaveFavorite(SongItem song)
        {
            var favSong = _musicService.SaveFavorite(song);
            var directoryResult = _musicService.GetDirectory(favSong.Path.GetParentDirectory());

            if (directoryResult is null) return false;

            Application.Current.Dispatcher.Invoke(() =>
            {
                _directoryTree.Value.Insert(directoryResult.Directories);
            });

            _directoryTree.OnNext(_directoryTree.Value);

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
            var parentPath = _currentSong.Value.Path.GetParentDirectory();
            var directoryResult = _musicService.GetDirectory(parentPath);

            if(directoryResult is null || _currentTime >= TimeSpan.FromSeconds(3))
            {
                LoadSong(_currentSong.Value);
                return;
            }
            var songIndex = directoryResult.Songs.IndexOf(_currentSong.Value);

            var songToLoad = songIndex == 0
                ? directoryResult.Songs.Last()
                : directoryResult.Songs[--songIndex];
            LoadSong(songToLoad);

        }

        public void PlayNext()
        {
            var parentPath = _currentSong.Value.Path.GetParentDirectory();
            var directoryResult = _musicService.GetDirectory(parentPath);

            if (directoryResult is null)
            {
                LoadSong(_currentSong.Value);
                return;
            }
            var currentIndex = directoryResult.Songs.IndexOf(_currentSong.Value);

            var songToLoad = directoryResult.Songs.Count == currentIndex + 1
                ? directoryResult.Songs.First()
                : directoryResult.Songs[++currentIndex];
            
            LoadSong(songToLoad);
        }

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
            var directoryResult = _musicService.GetDirectory(path);

            if(directoryResult is null)
            {
                _directoryLoading.OnNext(false);
                return Unit.Default;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                _directoryTree.Value.Insert(directoryResult.Directories);
                _directoryTree.Value.SelectDirectory(path);
            });

            _directoryTree.OnNext(_directoryTree.Value);

            var directoryItems = new ObservableCollection<StorageItem>();
            directoryItems.AddRange(directoryResult.Directories);
            directoryItems.AddRange(directoryResult.Songs);

            _directoryContent.OnNext(directoryItems);
            _currentDirectory.OnNext(directoryResult);
            _directoryTree.OnNext(_directoryTree.Value);
            _directoryLoading.OnNext(false);

            return Unit.Default;
        }

        private PlayState GetToggledPlayState() => _playState.Value == PlayState.Playing
                ? PlayState.Paused
                : PlayState.Playing;

        public void Dispose()
        {
            _currentTimeSubscription?.Dispose();
            _playingSongSubscription?.Dispose();
        }

        public IObservable<Unit> RefreshDirectory()
        {
            if (_currentDirectory.Value is null) return Observable.Empty<Unit>();

            _musicService.ClearCache(_currentDirectory.Value.Path);
            return LoadDirectory(_currentDirectory.Value.Path); 
        }
    }
}
