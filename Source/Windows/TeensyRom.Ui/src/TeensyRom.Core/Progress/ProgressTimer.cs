using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace TeensyRom.Core.Progress
{
    public interface IProgressTimer
    {
        IObservable<TimeSpan> CurrentTime { get; }
        IObservable<Unit> TimerComplete { get; }

        void StartNewTimer(TimeSpan length);
        void PauseTimer();
        void ResumeTimer();
    }
    public class ProgressTimer : IProgressTimer, IDisposable
    {
        public IObservable<TimeSpan> CurrentTime => _timeIntervalObservable.Select(_ => _currentTime);
        public IObservable<Unit> TimerComplete => _timerComplete.AsObservable();

        private Subject<Unit> _timerComplete = new();
        private IObservable<long> _timeIntervalObservable;
        private IDisposable? _timerSubscription;
        private IDisposable? _currentTimeSubscription;

        private TimeSpan _currentTime = TimeSpan.Zero;
        private TimeSpan _length;
        private TimeSpan _timeLeft;

        public ProgressTimer()
        {
            _timeIntervalObservable = Observable.Interval(TimeSpan.FromMilliseconds(20))
                .Publish()
                .RefCount();
        }
        public void StartNewTimer(TimeSpan length)
        {
            _currentTime = TimeSpan.Zero;
            _length = length;
            TryStopObservables();
            StartObservables(length);
        }
        public void PauseTimer()
        {
            _timeLeft = _length.Subtract(_currentTime);
            TryStopObservables();
        }
        public void ResumeTimer()
        {
            StartObservables(_timeLeft);
        }

        private void StartObservables(TimeSpan length)
        {
            _timerSubscription = Observable.Timer(length)
                .Subscribe(_ => _timerComplete.OnNext(Unit.Default));

            _currentTimeSubscription = _timeIntervalObservable
                .Subscribe(_ => _currentTime = _currentTime.Add(TimeSpan.FromMilliseconds(20)));
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