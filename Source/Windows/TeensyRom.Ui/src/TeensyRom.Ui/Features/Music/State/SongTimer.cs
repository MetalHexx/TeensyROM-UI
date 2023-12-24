using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace TeensyRom.Ui.Features.Music.State
{
    public interface ISongTimer
    {
        IObservable<TimeSpan> CurrentTime { get; }
        IObservable<Unit> SongComplete { get; }

        void StartNewTimer(TimeSpan songLength);
        void PauseTimer();
        void ResumeTimer();
    }
    public class SongTimer : ISongTimer, IDisposable
    {
        public IObservable<TimeSpan> CurrentTime => _timeIntervalObservable.Select(_ => _currentTime);
        public IObservable<Unit> SongComplete => _songComplete.AsObservable();

        private Subject<Unit> _songComplete = new();
        private IObservable<long> _timeIntervalObservable;
        private IDisposable? _timerSubscription;
        private IDisposable? _currentTimeSubscription;

        private TimeSpan _currentTime = TimeSpan.Zero;
        private TimeSpan _songLength;
        private TimeSpan _songTimeLeft;

        public SongTimer()
        {
            _timeIntervalObservable = Observable.Interval(TimeSpan.FromSeconds(1))
                .Publish()
                .RefCount();
        }
        public void StartNewTimer(TimeSpan songLength)
        {
            _currentTime = TimeSpan.Zero;
            _songLength = songLength;
            TryStopObservables();
            StartObservables(songLength);
        }
        public void PauseTimer()
        {
            _songTimeLeft = _songLength.Subtract(_currentTime);
            TryStopObservables();
        }
        public void ResumeTimer()
        {
            StartObservables(_songTimeLeft);
        }

        private void StartObservables(TimeSpan length)
        {
            _timerSubscription = Observable.Timer(length)
                .Subscribe(_ => _songComplete.OnNext(Unit.Default));

            _currentTimeSubscription = _timeIntervalObservable
                .Subscribe(_ => _currentTime = _currentTime.Add(TimeSpan.FromSeconds(1)));
        }

        private void TryStopObservables()
        {
            _timerSubscription?.Dispose();
            _timerSubscription = null;
            _currentTimeSubscription?.Dispose();
            _currentTimeSubscription = null;
        }

        public void Dispose()
        {
            TryStopObservables();
        }
    }
}