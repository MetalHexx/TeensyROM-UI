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

        Unit LoadDirectory(string path);
        Unit LoadSong(SongItemVm song);
        void PlayNext();
        void PlayPrevious();
    }

    public class MusicState : IMusicState
    {
        public IObservable<IEnumerable<StorageItemVm>> DirectoryContent => _directoryContent.AsObservable();
        public IObservable<SongItemVm> CurrentSong => _currentSong.AsObservable();
        public IObservable<TimeSpan> CurrentSongTime => _songTime.CurrentTime;
        public IObservable<SongMode> CurrentSongMode => _songMode.AsObservable();
        public IObservable<int> Skip => _skip.AsObservable();
        public IObservable<int> Take => _take.AsObservable();
        

        private BehaviorSubject<SongMode> _songMode = new(SongMode.Next);
        private readonly BehaviorSubject<SongItemVm> _currentSong = new(new()
        {
            Path = "/sync/sid/Aces_High.sid",
            ArtistName = "Iron Maiden",
            SongLength = TimeSpan.FromMinutes(1),
            Name = "Aces_High.sid"

        });
        private readonly Subject<IEnumerable<StorageItemVm>> _directoryContent = new();
        private readonly BehaviorSubject<int> _take = new(250);
        private readonly BehaviorSubject<int> _skip = new(0);
        private List<StorageItemVm> _storageItems = new List<StorageItemVm>();
        private List<SongItemVm> _songs = new List<SongItemVm>();


        private readonly ISongTimer _songTime;
        private readonly ILaunchFileCommand _launchFileCommand;
        private readonly IGetDirectoryCommand _getDirectoryCommand;
        private IDisposable? _playingSongSubscription;

        public MusicState(ISongTimer songTime, ILaunchFileCommand launchFileCommand, IGetDirectoryCommand getDirectoryCommand)
        {
            _songTime = songTime;
            _launchFileCommand = launchFileCommand;
            _getDirectoryCommand = getDirectoryCommand;
        }

        public void SetSkip(int skip) => _skip.OnNext(skip);

        public void SetTake(int take) => _take.OnNext(take);

        public void SetSongMode(SongMode songMode) => _songMode.OnNext(songMode);

        public Unit LoadSong(SongItemVm song)
        {
            _songTime.Reset();
            _songTime.Start();
            _launchFileCommand.Execute(song.Path);            

            _currentSong.OnNext(song);

            _playingSongSubscription?.Dispose();

            _playingSongSubscription = Observable.Timer(song.SongLength)
                .Subscribe(_ => PlayNext());

            return Unit.Default;
        }

        public void PlayPrevious()
        {
            var currentSong = _songs.First(s => s.Path == _currentSong.Value.Path);
            var currentIndex = _songs.IndexOf(currentSong);
            var nextSong = currentIndex == 0
                ? _songs.Last()
                : _songs[--currentIndex];

            LoadSong(nextSong);
        }

        public void PlayNext()
        {
            var currentSong = _songs.First(s => s.Path == _currentSong.Value.Path);
            var currentIndex = _songs.IndexOf(currentSong);
            var nextSong = _storageItems.Count == currentIndex + 1
                ? _songs.First()
                : _songs[++currentIndex];

            LoadSong(nextSong);
        }

        public Unit LoadDirectory(string path)
        {
            var directoryContent = _getDirectoryCommand.Execute(path, 0, 5000);

            if (directoryContent is null) return Unit.Default;

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
            return Unit.Default;
        }
    }
}
