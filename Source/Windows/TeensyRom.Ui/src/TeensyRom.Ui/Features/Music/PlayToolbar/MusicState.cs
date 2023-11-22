using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Commands.File.LaunchFile;
using TeensyRom.Ui.Features.Common.Models;

namespace TeensyRom.Ui.Features.Music.PlayToolbar
{
    public interface IMusicState
    {
        IObservable<IEnumerable<StorageItemVm>> DirectoryContent { get; }
        IObservable<SongItemVm> CurrentSong { get; }
        IObservable<SongMode> CurrentSongMode { get; }
        IObservable<TimeSpan> CurrentSongTime { get; }
        IObservable<PlayState> PlayState { get; }

        bool LoadDirectory(string path);
        bool LoadSong(SongItemVm song);
        bool PlayNext();
        bool PlayPrevious();
        bool ToggleMusic();
    }

    public class MusicState : IMusicState
    {
        public IObservable<IEnumerable<StorageItemVm>> DirectoryContent => _directoryContent.AsObservable();
        public IObservable<SongItemVm> CurrentSong => _currentSong.AsObservable();
        public IObservable<TimeSpan> CurrentSongTime => _songTime.CurrentTime;
        public IObservable<SongMode> CurrentSongMode => _songMode.AsObservable();
        public IObservable<PlayState> PlayState => _playState.AsObservable();
        public IObservable<int> Skip => _skip.AsObservable();
        public IObservable<int> Take => _take.AsObservable();
        

        private BehaviorSubject<SongMode> _songMode = new(SongMode.Next);
        private readonly BehaviorSubject<SongItemVm> _currentSong = new(new());
        private readonly Subject<IEnumerable<StorageItemVm>> _directoryContent = new();
        private readonly BehaviorSubject<int> _take = new(250);
        private readonly BehaviorSubject<int> _skip = new(0);
        private readonly BehaviorSubject<PlayState> _playState = new(PlayToolbar.PlayState.Paused);
        private List<StorageItemVm> _storageItems = new List<StorageItemVm>();
        private List<SongItemVm> _songs = new List<SongItemVm>();


        private readonly ISongTimer _songTime;
        private readonly ILaunchFileCommand _launchFileCommand;
        private readonly IGetDirectoryCommand _getDirectoryCommand;
        private readonly IToggleMusicCommand _toggleMusicCommand;
        private IDisposable? _playingSongSubscription;

        public MusicState(ISongTimer songTime, ILaunchFileCommand launchFileCommand, IGetDirectoryCommand getDirectoryCommand, IToggleMusicCommand toggleMusicCommand)
        {
            _songTime = songTime;
            _launchFileCommand = launchFileCommand;
            _getDirectoryCommand = getDirectoryCommand;
            _toggleMusicCommand = toggleMusicCommand;
        }

        public void SetSkip(int skip) => _skip.OnNext(skip);

        public void SetTake(int take) => _take.OnNext(take);

        public void SetSongMode(SongMode songMode) => _songMode.OnNext(songMode);

        public bool LoadSong(SongItemVm song)
        {
            _songTime.Reset();
            _songTime.Start();

            if (!_launchFileCommand.Execute(song.Path))
            {                
                return false;
            }
            _currentSong.OnNext(song);
            _playState.OnNext(PlayToolbar.PlayState.Playing);

            _playingSongSubscription?.Dispose();

            _playingSongSubscription = Observable.Timer(song.SongLength)
                .Subscribe(_ => PlayNext());

            return true;
        }
        public bool ToggleMusic()
        {   
            if (_toggleMusicCommand.Execute())
            {
                _playState.OnNext(GetToggledPlayState());
                return true;
            }
            return false;
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

        private PlayState GetToggledPlayState() => _playState.Value == PlayToolbar.PlayState.Playing
                ? PlayToolbar.PlayState.Paused
                : PlayToolbar.PlayState.Playing;

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
                .Select(f => new SongItemVm
                {
                    ArtistName = string.Empty,
                    Name = f.Name,
                    Path = f.Path,
                    Size = f.Size
                })
                .OrderBy(f => f.Name);


            var directories = directoryContent.Directories
                .Select(d => new DirectoryItemVm
                {
                    Name = d.Name,
                    Path = d.Path
                })
                .OrderBy(d => d.Name);

            _storageItems.Clear();
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
