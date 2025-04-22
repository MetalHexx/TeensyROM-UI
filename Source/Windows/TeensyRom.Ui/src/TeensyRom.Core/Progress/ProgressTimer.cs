using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace TeensyRom.Ui.Core.Progress
{
    public interface IProgressTimer
    {
        IObservable<TimeSpan> CurrentTime { get; }
        IObservable<Unit> TimerComplete { get; }

        void StartNewTimer(TimeSpan length);
        void PauseTimer();
        void ResumeTimer();
        void ResetTimer();
        void UpdateSpeed(double speedPercent);
    }
    public class ProgressTimer : IProgressTimer, IDisposable
    {
        public IObservable<TimeSpan> CurrentTime => _timeIntervalObservable.Select(_ => _currentTime);
        public IObservable<Unit> TimerComplete => _timerComplete.AsObservable();

        private Subject<Unit> _timerComplete = new();
        private IObservable<long> _timeIntervalObservable;
        private IDisposable? _currentTimeSubscription;

        private TimeSpan _currentTime = TimeSpan.Zero;
        private TimeSpan _length;
        private TimeSpan _timeLeft;
        private double _speedMultiplier = 1.0;
        private readonly Stopwatch _stopwatch = new();

        public ProgressTimer()
        {
            _timeIntervalObservable = Observable.Interval(TimeSpan.FromMilliseconds(20))
                .Publish()
                .RefCount();
        }

        public void StartNewTimer(TimeSpan length)
        {
            _currentTime = TimeSpan.Zero;
            _timeLeft = length;
            _length = length;
            _speedMultiplier = 1;
            TryStopObservables();
            StartObservables();
        }

        public void PauseTimer() => TryStopObservables();

        public void ResumeTimer()
        {
            TryStopObservables();
            StartObservables();
        }

        public void ResetTimer() 
        {
            _currentTime = TimeSpan.Zero;
            _timeLeft = _length;
            TryStopObservables();
            StartObservables();
        }

        public void UpdateSpeed(double percentage)
        {
            if (percentage < 0)
            {
                percentage = percentage * -1;
                _speedMultiplier = 1.0 - (percentage / 100.0);
            }
            else
            {
                _speedMultiplier = 1.0 + (percentage / 100.0);
            }
            if (_speedMultiplier <= 0)
            {
                _speedMultiplier = 0.01;
            }
        }

        private void StartObservables()
        {
            _stopwatch.Restart();

            _currentTimeSubscription = Observable.Interval(TimeSpan.FromMilliseconds(20))
                .Subscribe(_ =>
                {
                    var timeStep = TimeSpan.FromMilliseconds(_stopwatch.ElapsedMilliseconds * _speedMultiplier);
                    _currentTime = _currentTime.Add(timeStep);
                    _timeLeft = _length.Subtract(_currentTime);
                    _stopwatch.Restart();

                    if (_currentTime >= _length) 
                    { 
                        TryStopObservables();
                        _stopwatch.Reset();
                        _timerComplete.OnNext(Unit.Default);
                    }
                });
        }

        private void TryStopObservables()
        {
            _currentTimeSubscription?.Dispose();
            _currentTimeSubscription = null;
        }

        public void Dispose()
        {
            TryStopObservables();
        }
    }
}