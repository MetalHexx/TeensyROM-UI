using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using TeensyRom.Core.Commands.File.LaunchFile;

namespace TeensyRom.Ui.Features.Music.PlayToolbar
{
    public interface IMusicState
    {
        IObservable<Song> CurrentSong { get; }
        IObservable<SongMode> CurrentSongMode { get; }
        IObservable<TimeSpan> CurrentSongTime { get; }

        Unit LoadSong(Song song);
    }

    public class MusicState : IMusicState
    {
        public IObservable<Song> CurrentSong => _currentSong.AsObservable();
        public IObservable<TimeSpan> CurrentSongTime => _songTime.CurrentTime;
        public IObservable<SongMode> CurrentSongMode => _songMode.AsObservable();

        private BehaviorSubject<SongMode> _songMode = new(SongMode.Next);
        private readonly BehaviorSubject<Song> _currentSong = new(new());
        private readonly ISongTimer _songTime;
        private readonly ILaunchFileCommand _launchFileCommand;
        private IDisposable? _playingSongSubscription;

        public MusicState(ISongTimer songTime, ILaunchFileCommand launchFileCommand)
        {
            _songTime = songTime;
            _launchFileCommand = launchFileCommand;
        }

        public Unit LoadSong(Song song)
        {
            _launchFileCommand.Execute(song.Path);
            _songTime.Reset();
            _currentSong.OnNext(song);
            _songTime.Start();

            _playingSongSubscription?.Dispose();

            _playingSongSubscription = _songTime.CurrentTime
                .CombineLatest(_currentSong.DistinctUntilChanged(), (time, song) => (time, song))
                .CombineLatest(_songMode.DistinctUntilChanged(), (ts, mode) => (ts.time, ts.song, mode))
                .Where(tsm => tsm.time.CompareTo(tsm.song.SongLength) >= 0)
                //.Where(tsm => tsm.time.CompareTo(TimeSpan.FromSeconds(5)) >= 0)
                .Where(tsm => tsm.mode == SongMode.Next)
                .Subscribe(_ => LoadSong(song));

            return Unit.Default;
        }
    }
}
