using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Commands.File.LaunchFile;
using TeensyRom.Core.Music;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Features.Common.Models;

namespace TeensyRom.Ui.Features.Music.State
{
    public class MusicState : IMusicState
    {
        public IObservable<IEnumerable<StorageItem>> DirectoryContent => _directoryContent.AsObservable();
        public IObservable<SongItem> CurrentSong => _currentSong.AsObservable();
        public IObservable<TimeSpan> CurrentSongTime => _songTime.CurrentTime;
        public IObservable<SongMode> CurrentSongMode => _songMode.AsObservable();
        public IObservable<PlayState> CurrentPlayState => _playState.AsObservable();
        public IObservable<int> Skip => _skip.AsObservable();
        public IObservable<int> Take => _take.AsObservable();


        private readonly BehaviorSubject<SongMode> _songMode = new(SongMode.Next);
        private readonly BehaviorSubject<SongItem> _currentSong = new(new());
        private readonly Subject<IEnumerable<StorageItem>> _directoryContent = new();
        private readonly BehaviorSubject<int> _take = new(250);
        private readonly BehaviorSubject<int> _skip = new(0);
        private readonly BehaviorSubject<PlayState> _playState = new(PlayState.Paused);

        private readonly List<StorageItem> _storageItems = new();
        private readonly List<SongItem> _songs = new();
        private DirectoryItem _directoryTree = new();

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
            _settingsService.Settings.Subscribe(settings => 
            {
                _settings = settings;
                _directoryTree = new DirectoryItem
                {
                    Name = _settings.GetFileTypePath(TeensyFileType.Sid),
                    Path = _settings.GetFileTypePath(TeensyFileType.Sid)
                };
            });
        }

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

            if (LoadSong(nextSong))
            {
                return true;
            }
            return false;
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

        public bool LoadDirectory(string path)
        {
            var directoryContent = _getDirectoryCommand.Execute(path, 0, 5000);

            if (directoryContent is null) return false;

            var songs = directoryContent.Files
                .Select(f =>
                {
                    var defaultSidPath = _settings.GetFileTypePath(TeensyFileType.Sid);
                    var trimmedPath = f.Path.Replace($"{defaultSidPath}/hvsc", "");
                    var sidRecord = _musicService.Find(trimmedPath);

                    return new SongItem
                    {
                        ArtistName = sidRecord?.Author ?? "Unknown",
                        Name = sidRecord?.Title ?? f.Name,
                        Path = f.Path,
                        Size = f.Size,
                        SongLength = sidRecord?.SongLengthSpan ?? MusicConstants.DefaultLength

                    };
                })
                .OrderBy(f => f.Name);

            var directories = directoryContent.Directories
                .Select(d => new DirectoryItem
                {
                    Name = d.Name,
                    Path = d.Path
                })
                .OrderBy(d => d.Name);

            _storageItems.Clear();
            _directoryTree.AddRange(directories);
            _storageItems.AddRange(directories);
            _storageItems.AddRange(songs);
            _songs.Clear();
            _songs.AddRange(songs);

            _skip.OnNext(0);

            _directoryContent.OnNext(_storageItems.Skip(_skip.Value).Take(_take.Value));
            return true;
        }
    }
}
