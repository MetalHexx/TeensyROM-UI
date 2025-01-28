using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using TeensyRom.Core.Common;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Core.Storage.Services;
using TeensyRom.Ui.Features.Discover.State;
using TeensyRom.Ui.Core.Progress;
using System.Reactive.Concurrency;
using TeensyRom.Core.Music;

namespace TeensyRom.Ui.Controls.PlayToolbar
{
    public enum FastForwardSpeed 
    {
        Off,
        Medium, 
        MediumFast,
        Fast,
        Hyper
    }
    public class PlayToolbarViewModel : ReactiveObject
    {
        [Reactive] public bool PlayButtonEnabled { get; set; }
        [Reactive] public bool PauseButtonEnabled { get; set; }        
        [Reactive] public bool StopButtonEnabled { get; set; }        
        [Reactive] public bool TimedPlayEnabled { get; set; }
        [Reactive] public bool TimedPlayButtonEnabled { get; set; }
        [Reactive] public bool TimedPlayComboBoxEnabled { get; set; }
        [Reactive] public bool ProgressEnabled { get; set; }                
        [Reactive] public string TimerSeconds { get; set; } = "3m";
        [Reactive] public List<string> TimerOptions { get; set; } = ["5s", "10s", "15s", "30s", "1m", "3m", "5m", "10m", "15m", "30m"];
        [Reactive] public bool FastForwardEnabled { get; set; }
        [Reactive] public FastForwardSpeed FastForwardSpeed { get; set; } = FastForwardSpeed.Off;
        [Reactive] public bool SetSpeedEnabled { get; set; }
        [Reactive] public double RawSpeedValue { get; set; }
        [Reactive] public double ActualSpeedPercent { get; set; }
        [Reactive] public List<MusicSpeedCurveTypes> SpeedCurveOptions { get; set; } = [MusicSpeedCurveTypes.Linear, MusicSpeedCurveTypes.Logarithmic];
        [Reactive] public MusicSpeedCurveTypes SelectedSpeedCurve { get; set; } = MusicSpeedCurveTypes.Linear;
        [Reactive] public double MinSpeed { get; set; } = linearMin;
        [Reactive] public double MaxSpeed { get; set; } = linearMax;        
        [Reactive] public string SelectedScope { get; set; } = StorageScope.Storage.ToDescription();

        [Reactive]
        public List<string> ScopeOptions { get; set; } = Enum
            .GetValues(typeof(StorageScope))
            .Cast<StorageScope>().ToList()
            .Select(e => e.ToDescription())
            .ToList();
        [Reactive] public int CurrentSubtuneIndex { get; set; }
        [Reactive] public bool SubtuneNextButtonEnabled { get; set; }
        [Reactive] public bool SubtunePreviousButtonEnabled { get; set; }
        [ObservableAsProperty] public bool SubtunesEnabled { get; }
        [ObservableAsProperty] public bool IsSong { get; }
        [ObservableAsProperty] public List<int> SubtuneIndex { get; }
        [ObservableAsProperty] public bool ShowTitleOnly { get; }
        [ObservableAsProperty] public string StorageScopePath { get; }
        [ObservableAsProperty] public ILaunchableItem? File { get; }
        [ObservableAsProperty] public TimeProgressViewModel? Progress { get; } = null;
        [ObservableAsProperty] public bool ShuffleModeEnabled { get; }
        [ObservableAsProperty] public bool ShareVisible { get; }        
        [ObservableAsProperty] public bool ShowCreator { get; }
        [ObservableAsProperty] public bool ShowReleaseInfo { get; }
        [ObservableAsProperty] public bool ShowReleaseCreatorSeperator { get; }        

        public ReactiveCommand<Unit, Unit> TogglePlayCommand { get; set; }
        public ReactiveCommand<Unit, Unit> PreviousCommand { get; set; }
        public ReactiveCommand<Unit, Unit> NextCommand { get; set; }
        public ReactiveCommand<Unit, Unit> FastForwardCommand { get; set; }
        public ReactiveCommand<Unit, Unit> ToggleShuffleCommand { get; set; }
        public ReactiveCommand<Unit, Unit> ToggleTimedPlay { get; set; }
        public ReactiveCommand<Unit, Unit> FavoriteCommand { get; set; }
        public ReactiveCommand<Unit, Unit> RemoveFavoriteCommand { get; set; }
        public ReactiveCommand<Unit, Unit> ShareCommand { get; set; }
        public ReactiveCommand<Unit, Unit> NavigateToFileDirCommand { get; set; }
        public ReactiveCommand<Unit, Unit> NextSubtuneCommand { get; set; }
        public ReactiveCommand<Unit, Unit> PreviousSubtuneCommand { get; set; }
        public ReactiveCommand<double, Unit> SetSpeedCommand { get; set; }

        private readonly IAlertService _alert;
        private IProgressTimer? _timer;
        private IDisposable? _timerCompleteSubscription;
        private IDisposable? _currentTimeSubscription;
        private double _previousRawSpeed = 0;
        private MusicSpeedCurveTypes _previousSpeedCurve = MusicSpeedCurveTypes.Linear;
        private const double logMin = -127;
        private const double logMax = 99;
        private const double linearMin = -50;
        private const double linearMax = 125;

        public PlayToolbarViewModel(
            IObservable<ILaunchableItem> file,
            IObservable<LaunchItemState> playState,
            IObservable<bool> timedPlayEnabled,
            IObservable<StorageScope> storageScope,
            IObservable<string> storageScopePath,
            IProgressTimer? timer,
            Func<Unit> toggleMode,
            Func<Task> togglePlay,
            Func<Task> playPrevious,
            Func<Task> playNext,
            Func<int, Task> playSubtune,
            Func<ILaunchableItem, Task> saveFav,
            Func<ILaunchableItem, Task> removeFav,
            Func<string, Task> loadDirectory,
            Func<double, MusicSpeedCurveTypes, Task> changeSpeed,
            Action<StorageScope> setScope,
            IAlertService alert)
        {
            _timer = timer;
            _alert = alert;

            var currentFile = file.Where(item => item is not null);

            var showReleaseInfo = currentFile
                .Select(item => !string.IsNullOrWhiteSpace(item.ReleaseInfo));

            var showCreatorInfo = currentFile
                .Select(item => !string.IsNullOrWhiteSpace(item.Creator));

            currentFile.Subscribe(_ => DisableFastForward());

            currentFile.ToPropertyEx(this, f => f.File);

            showReleaseInfo.ToPropertyEx(this, vm => vm.ShowReleaseInfo);

            showCreatorInfo.ToPropertyEx(this, vm => vm.ShowCreator);

            showReleaseInfo.CombineLatest(showCreatorInfo, (release, creator) => release && creator)
                .ToPropertyEx(this, vm => vm.ShowReleaseCreatorSeperator);

            showReleaseInfo.CombineLatest(showCreatorInfo, (release, creator) => !(release || creator))
                .Select(x => x)
                .ToPropertyEx(this, vm => vm.ShowTitleOnly);

            currentFile
                .Select(f => !string.IsNullOrWhiteSpace(f.ShareUrl))
                .ToPropertyEx(this, vm => vm.ShareVisible);

            currentFile.Subscribe(_ => RawSpeedValue = 0);

            playState
                .Select(state => state.PlayMode == PlayMode.Shuffle)
                .ToPropertyEx(this, vm => vm.ShuffleModeEnabled);

            storageScopePath
                .ObserveOn(RxApp.MainThreadScheduler)
                .Skip(1)
                .Subscribe(_ => SelectedScope = StorageScope.DirDeep.ToDescription());

            storageScope
                .CombineLatest(storageScopePath, (scope, path) => (scope, path))
                .Select(scopeAndPath =>
                {
                    if (scopeAndPath.path == StorageConstants.Remote_Path_Root) return scopeAndPath;

                    if (scopeAndPath.path.Length <= 10) return scopeAndPath;

                    var rangeToTake = Math.Abs(24 - scopeAndPath.path.Length);

                    scopeAndPath.path = $"...{scopeAndPath.path[rangeToTake..]}";
                    return scopeAndPath;
                })
                .Select(scopeAndPath => scopeAndPath.scope switch
                {
                    StorageScope.DirDeep => scopeAndPath.path,
                    StorageScope.DirShallow => scopeAndPath.path,
                    _ => string.Empty
                })
                .ToPropertyEx(this, vm => vm.StorageScopePath);

            playState
                .Where(state => state.PlayState != PlayState.Playing)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ =>
                {
                    PauseButtonEnabled = false;
                    StopButtonEnabled = false;
                    PlayButtonEnabled = true;
                    _timer?.PauseTimer();
                });

            var playToggle = playState
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(state => state.PlayState == PlayState.Playing)
                .Do(_ => PlayButtonEnabled = false);

            playToggle
                .Where(_ => File is GameItem or HexItem or ImageItem)
                .Subscribe(item =>
                {
                    StopButtonEnabled = true;
                    PauseButtonEnabled = false;
                });

            playToggle
                .Where(_ => File is SongItem)
                .Subscribe(_ =>
                {
                    StopButtonEnabled = false;
                    PauseButtonEnabled = true;
                });

            var song = currentFile.OfType<SongItem>().ObserveOn(RxApp.MainThreadScheduler);
            var hexItem = currentFile.OfType<HexItem>().ObserveOn(RxApp.MainThreadScheduler);
            var gameOrImage = currentFile.Where(f => f is GameItem or ImageItem).ObserveOn(RxApp.MainThreadScheduler);

            file.Select(file => file is SongItem song && song.SubtuneLengths.Count > 1)
                .ToPropertyEx(this, vm => vm.SubtunesEnabled);

            file.Select(f => f is SongItem)
                .ToPropertyEx(this, vm => vm.IsSong);

            file.Select(f => f is SongItem)
                .Subscribe(isSong => SetSpeedEnabled = isSong);

            song.Select(s => s.StartSubtuneNum)
                .Subscribe(startIndex =>
                {
                    CurrentSubtuneIndex = startIndex;
                });

            song.Select(s => s.SubtuneLengths.Select((_, i) => i + 1).ToList())
                .Where(lengths => lengths.Count > 1)
                .ToPropertyEx(this, vm => vm.SubtuneIndex);

            song.Subscribe(item =>
            {
                TimedPlayButtonEnabled = false;
                TimedPlayComboBoxEnabled = false;
                ProgressEnabled = true;
                InitializeProgress(playNext, item);
            });


            gameOrImage
                .Where(_ => TimedPlayEnabled)
                .Subscribe(item =>
                {
                    TimedPlayButtonEnabled = true;
                    TimedPlayComboBoxEnabled = true;
                    ProgressEnabled = true;
                    InitializeProgress(playNext, item);
                });

            gameOrImage
                .Where(_ => !TimedPlayEnabled)
                .Subscribe(item =>
                {
                    TimedPlayButtonEnabled = true;
                    TimedPlayComboBoxEnabled = false;
                    ProgressEnabled = false;
                    _timer?.PauseTimer();
                });

            hexItem.Subscribe(item =>
            {
                TimedPlayButtonEnabled = false;
                TimedPlayComboBoxEnabled = false;
                ProgressEnabled = false;
                _timer?.PauseTimer();
            });

            this.WhenAnyValue(x => x.TimerSeconds)
                .Where(_ => TimedPlayEnabled)
                .Subscribe(timeSpan =>
                {
                    InitializeProgress(playNext, File);
                });

            this.WhenAnyValue(x => x.SelectedScope)
                .Select(s => s.ToEnum<StorageScope>())
                .Subscribe(scope => setScope(scope));

            timedPlayEnabled
                .Take(2)
                .Where(pt => pt is true).Subscribe(enabled =>
                {
                    TimedPlayEnabled = true;
                    TimedPlayComboBoxEnabled = true;
                    ProgressEnabled = true;
                    InitializeProgress(playNext, (File));
                });

            TogglePlayCommand = ReactiveCommand.CreateFromTask(_ =>
            {
                if (FastForwardEnabled)
                {
                    DisableFastForward(true);
                    return Task.CompletedTask;
                }
                if (PlayButtonEnabled) _timer?.ResumeTimer();
                if (PauseButtonEnabled) _timer?.PauseTimer();
                return togglePlay();
            });

            ToggleTimedPlay = ReactiveCommand.Create<Unit>(_ =>
            {
                TimedPlayEnabled = !TimedPlayEnabled;

                if (TimedPlayEnabled)
                {
                    TimedPlayComboBoxEnabled = true;
                    ProgressEnabled = true;
                    InitializeProgress(playNext, (File));
                    return;
                }
                TimedPlayComboBoxEnabled = false;
                ProgressEnabled = false;
                _timer?.PauseTimer();
            });

            FastForwardCommand = ReactiveCommand.Create(() =>
            {
                switch (FastForwardSpeed)
                {
                    case FastForwardSpeed.Off:
                        PauseButtonEnabled = false;
                        PlayButtonEnabled = true;
                        FastForwardEnabled = true;
                        SetSpeedEnabled = false;
                        FastForwardSpeed = FastForwardSpeed.Medium;
                        _previousRawSpeed = RawSpeedValue;
                        _previousSpeedCurve = SelectedSpeedCurve;
                        SelectedSpeedCurve = MusicSpeedCurveTypes.Logarithmic;
                        RawSpeedValue = 25;
                        return;

                    case FastForwardSpeed.Medium:
                        FastForwardSpeed = FastForwardSpeed.MediumFast;
                        RawSpeedValue = 50;
                        return;

                    case FastForwardSpeed.MediumFast:
                        FastForwardSpeed = FastForwardSpeed.Fast;
                        RawSpeedValue = SelectedSpeedCurve == MusicSpeedCurveTypes.Linear ? 75 : 90;
                        return;

                    case FastForwardSpeed.Fast:
                        FastForwardSpeed = FastForwardSpeed.Hyper;
                        RawSpeedValue = SelectedSpeedCurve == MusicSpeedCurveTypes.Linear ? 125 : 96;
                        return;

                    case FastForwardSpeed.Hyper:
                        DisableFastForward(true);
                        return;
                }
            });
            NextCommand = ReactiveCommand.CreateFromTask(_ =>
            {
                return playNext();
            });
            PreviousCommand = ReactiveCommand.CreateFromTask(_ =>
            {
                return playPrevious();
            });
            ToggleShuffleCommand = ReactiveCommand.Create(toggleMode);
            FavoriteCommand = ReactiveCommand.CreateFromTask(_ => saveFav(File!));
            RemoveFavoriteCommand = ReactiveCommand.CreateFromTask(_ => removeFav(File!));
            ShareCommand = ReactiveCommand.Create<Unit, Unit>(_ => HandleShareCommand());
            NavigateToFileDirCommand = ReactiveCommand.CreateFromTask(_ => loadDirectory(File!.Path.GetUnixParentPath()!));            
            
            PreviousSubtuneCommand = ReactiveCommand.Create<Unit, Unit>(_ => 
            {
                var song = File as SongItem;

                if (CurrentSubtuneIndex == 1) 
                {
                    SubtunePreviousButtonEnabled = false;
                    return Unit.Default;
                }
                
                CurrentSubtuneIndex--;
                return Unit.Default;
            });
            NextSubtuneCommand = ReactiveCommand.Create<Unit, Unit>(_ =>
            {
                var song = File as SongItem;

                if (song is null || CurrentSubtuneIndex == song.SubtuneLengths.Count) return Unit.Default;

                if (CurrentSubtuneIndex == song.SubtuneLengths.Count);
                CurrentSubtuneIndex++;
                return Unit.Default;
            });

            this.WhenAnyValue(x => x.SelectedSpeedCurve)
                .Skip(1)
                .Subscribe(x => 
                {
                    _previousSpeedCurve = FastForwardEnabled 
                        ? MusicSpeedCurveTypes.Linear 
                        : SelectedSpeedCurve;

                    if (x == MusicSpeedCurveTypes.Linear)
                    {
                        MinSpeed = linearMin;
                        MaxSpeed = linearMax; 
                    }
                    else
                    {
                        MinSpeed = logMin;
                        MaxSpeed = logMax;     
                    }
                    RawSpeedValue = 0;                    
                });

            var speedChanges = this.WhenAnyValue(x => x.RawSpeedValue)
                .Skip(1)
                .Do(rawSpeed => 
                {
                    ActualSpeedPercent = SelectedSpeedCurve == MusicSpeedCurveTypes.Linear
                        ? rawSpeed
                        : rawSpeed.GetLogPercentage();

                    timer?.UpdateSpeed(ActualSpeedPercent);
                })
                .Throttle(TimeSpan.FromMilliseconds(100))
                .DistinctUntilChanged()
                .ObserveOn(TaskPoolScheduler.Default)
                .Subscribe(async rawSpeed =>
                {   
                    await changeSpeed(rawSpeed, SelectedSpeedCurve).ConfigureAwait(false);                    
                });

            this.WhenAnyValue(x => x.CurrentSubtuneIndex)                
                .Skip(2)
                .Where(_ => File is SongItem item && item.SubtuneLengths.Count > 1)
                .Subscribe(songAndIndex =>
                {
                    InitializeSubtuneProgress(CurrentSubtuneIndex, playNext);
                    ResetSubtuneButtonState();
                    playSubtune(CurrentSubtuneIndex);
                });
        }

        private void DisableFastForward(bool usePrevious = false)
        {
            PlayButtonEnabled = false;
            PauseButtonEnabled = true;
            FastForwardEnabled = false;
            SetSpeedEnabled = true;
            FastForwardSpeed = FastForwardSpeed.Off;
            RawSpeedValue = usePrevious ? _previousRawSpeed : 0;
            SelectedSpeedCurve = _previousSpeedCurve;
        }

        private void ResetSubtuneButtonState()
        {
            var song = File as SongItem;

            if (song is null) return;
            
            SubtuneNextButtonEnabled = CurrentSubtuneIndex < song.SubtuneLengths.Count;
            SubtunePreviousButtonEnabled = CurrentSubtuneIndex > 1;
        }

        private void InitializeSubtuneProgress(int subtuneIndex, Func<Task> playNext) 
        {
            var song = File as SongItem;

            if (song == null || song.SubtuneLengths.Count == 0) return;

            var zeroBasedIndex = subtuneIndex > 0 ? subtuneIndex - 1 : 0;

            var subtuneLength = song.SubtuneLengths[zeroBasedIndex];

            StartTimerObservables(subtuneLength, playNext);
        }

        private void InitializeProgress(Func<Task> playNext, ILaunchableItem? item)
        {
            if (item == null) return;            

            TimeSpan playLength;

            if(item is SongItem songItem)
            {
                playLength = songItem.PlayLength;
                
                if (songItem.SubtuneLengths.Count > 1) 
                {
                    playLength = songItem.SubtuneLengths[songItem.StartSubtuneNum - 1];
                    ResetSubtuneButtonState();
                }
            }
            else
            {
                playLength = ConvertToTimeSpan(TimerSeconds);
            }
            StartTimerObservables(playLength, playNext);            
        }

        private void StartTimerObservables(TimeSpan timespan, Func<Task> onNext) 
        {
            if (_timer == null) return;

            _timerCompleteSubscription?.Dispose();
            _currentTimeSubscription?.Dispose();

            _timer?.StartNewTimer(timespan);

            _currentTimeSubscription = _timer?.CurrentTime
                .Select(t => new TimeProgressViewModel(timespan, t))
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, vm => vm.Progress);

            _timerCompleteSubscription = _timer?.TimerComplete
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ =>
                {
                    onNext();
                });
        }

        private Unit HandleShareCommand() 
        {   
            Clipboard.SetDataObject(File!.ShareUrl);
            _alert.Publish("Share URL copied to clipboard.");
            return Unit.Default;
        }

        private TimeSpan ConvertToTimeSpan(string timeString)
        {
            if (string.IsNullOrWhiteSpace(timeString))
            {
                return TimeSpan.Zero;
            }

            char timeUnit = timeString[^1];

            if(timeUnit != 's' && timeUnit != 'm')
            {
                return TimeSpan.Zero;
            }

            if (int.TryParse(timeString[..^1], out int timeValue))
            {
                return timeUnit switch
                {
                    's' => TimeSpan.FromSeconds(timeValue),
                    'm' => TimeSpan.FromMinutes(timeValue),
                    _ => TimeSpan.Zero
                };
            }
            return TimeSpan.Zero;
        }
    }
}
