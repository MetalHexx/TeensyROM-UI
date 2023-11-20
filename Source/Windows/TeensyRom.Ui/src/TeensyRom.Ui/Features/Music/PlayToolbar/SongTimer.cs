using System;
using System.Reactive.Linq;

namespace TeensyRom.Ui.Features.Music.PlayToolbar
{
    public interface ISongTimer
    {
        IObservable<TimeSpan> CurrentTime { get; }

        void Reset();
        void Start();
        void Stop();
    }
    public class SongTimer : ISongTimer
    {
        private readonly IObservable<long> _timerObservable;
        private IDisposable? _timerSubscription;
        private TimeSpan _currentTime = TimeSpan.Zero;

        public IObservable<TimeSpan> CurrentTime => _timerObservable.Select(_ => _currentTime);

        public SongTimer()
        {
            _timerObservable = Observable.Interval(TimeSpan.FromSeconds(1))
                .Publish()
                .RefCount();
        }

        public void Start()
        {
            _timerSubscription = _timerObservable.Subscribe(_ =>
            {
                _currentTime = _currentTime.Add(TimeSpan.FromSeconds(1));
            });
        }

        public void Stop()
        {
            _timerSubscription?.Dispose();
        }

        public void Reset()
        {
            Stop();
            _currentTime = TimeSpan.Zero;
        }
    }
}
