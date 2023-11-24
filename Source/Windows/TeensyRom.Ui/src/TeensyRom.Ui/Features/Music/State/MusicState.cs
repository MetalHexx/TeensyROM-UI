using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Threading;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Commands.File.LaunchFile;
using TeensyRom.Core.Common;
using TeensyRom.Core.Music;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Features.Common.Models;

namespace TeensyRom.Ui.Features.Music.State
{
    public class MusicState : IMusicState
    {
        public IObservable<DirectoryItem> DirectoryTree => _directoryTree.AsObservable();
        public IObservable<ObservableCollection<StorageItem>> DirectoryContent => _directoryContent.AsObservable();
        public IObservable<bool> DirectoryLoading => _directoryLoading.AsObservable();
        public IObservable<SongItem> CurrentSong => _currentSong.AsObservable();        
        public IObservable<SongMode> CurrentSongMode => _songMode.AsObservable();
        public IObservable<PlayState> CurrentPlayState => _playState.AsObservable();
        public IObservable<TimeSpan> CurrentSongTime => _songTime.CurrentTime;
        public IObservable<int> Skip => _skip.AsObservable();
        public IObservable<int> Take => _take.AsObservable();

        private readonly BehaviorSubject<DirectoryItem> _directoryTree = new(new());
        private readonly Subject<ObservableCollection<StorageItem>> _directoryContent = new();
        private readonly Subject<bool> _directoryLoading = new();
        private readonly BehaviorSubject<SongItem> _currentSong = new(null);
        private readonly BehaviorSubject<SongMode> _songMode = new(SongMode.Next);
        private readonly BehaviorSubject<PlayState> _playState = new(PlayState.Paused);
        private readonly BehaviorSubject<int> _take = new(250);
        private readonly BehaviorSubject<int> _skip = new(0);        

        private readonly List<StorageItem> _storageItems = new();
        private readonly List<SongItem> _songs = new();

        private readonly ISongTimer _songTime;
        private readonly ILaunchFileCommand _launchFileCommand;
        private readonly IGetDirectoryCommand _getDirectoryCommand;
        private readonly IToggleMusicCommand _toggleMusicCommand;
        private readonly IMusicService _musicService;
        private readonly ISettingsService _settingsService;
        private TeensySettings _settings = new();
        private IDisposable? _playingSongSubscription;

        public MusicState(ISongTimer songTime, ILaunchFileCommand launchFileCommand, IGetDirectoryCommand getDirectoryCommand, IToggleMusicCommand toggleMusicCommand, IMusicService musicService, ISettingsService settingsService)
        {
            _songTime = songTime;
            _launchFileCommand = launchFileCommand;
            _getDirectoryCommand = getDirectoryCommand;
            _toggleMusicCommand = toggleMusicCommand;
            _musicService = musicService;
            _settingsService = settingsService;
            _settingsService.Settings.Subscribe(settings => OnSettingsChanged(settings));          
        }        

        private void OnSettingsChanged(TeensySettings settings)
        {
            _settings = settings;
            ResetDirectoryTree();
        }

        private void ResetDirectoryTree() => _directoryTree.OnNext(new DirectoryItem
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
        });

        public void SetSkip(int skip) => _skip.OnNext(skip);

        public void SetTake(int take) => _take.OnNext(take);

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

        public bool PlayPrevious()
        {
            var currentSong = _songs.First(s => s.Path == _currentSong.Value.Path);
            var currentIndex = _songs.IndexOf(currentSong);
            var nextSong = currentIndex == 0
                ? _songs.Last()
                : _songs[--currentIndex];

            return LoadSong(nextSong);
        }

        private PlayState GetToggledPlayState() => _playState.Value == PlayState.Playing
                ? PlayState.Paused
                : PlayState.Playing;

        public bool PlayNext()
        {
            var currentSong = _songs.First(s => s.Path == _currentSong.Value.Path);
            var currentIndex = _songs.IndexOf(currentSong);

            var nextSong = _storageItems.Count == currentIndex + 1
                ? _songs.First()
                : _songs[++currentIndex];

            if (LoadSong(nextSong))
            {
                return true;
            }
            return false;
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

            var directoryContent = _getDirectoryCommand.Execute(path, 0, 5000);

            if (directoryContent is null)
            {
                _directoryLoading.OnNext(false);
                return Unit.Default;
            }

            var songs = directoryContent.Files
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
                .OrderBy(song => song.Name);

            var directories = directoryContent.Directories
                .Select(d => new DirectoryItem
                {
                    Name = d.Name,
                    Path = d.Path
                })
                .OrderBy(d => d.Name);

            _storageItems.Clear();
            _songs.Clear();
            _songs.AddRange(songs);



            Application.Current.Dispatcher.Invoke(() =>
            {
                _directoryTree.Value.Insert(directories);
                _storageItems.AddRange(directories);
                _storageItems.AddRange(songs);
            });

            

            _directoryTree.OnNext(_directoryTree.Value);
            _skip.OnNext(0);

            var dirContent = new ObservableCollection<StorageItem>( _storageItems
                .Skip(_skip.Value)
                .Take(_take.Value));

            _directoryContent.OnNext(dirContent);
            _directoryLoading.OnNext(false);

            return Unit.Default;
        }
    }
}
