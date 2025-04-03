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
using TeensyRom.Ui.Services;
using TeensyRom.Ui.Main;
using TeensyRom.Core.Music.Midi;
using TeensyRom.Core.Settings;
using TeensyRom.Ui.Controls.Playlist;
using System.Runtime.CompilerServices;

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

    public enum SeekSpeed 
    {
        Accurate,
        Insane
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
        [Reactive] public bool FastForwardInProgress { get; set; }
        [Reactive] public bool TrackSeekInProgress { get; set; }
        [Reactive] public FastForwardSpeed FastForwardSpeed { get; set; } = FastForwardSpeed.Off;
        [Reactive] public bool SetSpeedEnabled { get; set; }
        [Reactive] public double RawSpeedValue { get; set; }
        [Reactive] public double ActualSpeedPercent { get; set; }
        [Reactive] public List<MusicSpeedCurveTypes> SpeedCurveOptions { get; set; } = [MusicSpeedCurveTypes.Linear, MusicSpeedCurveTypes.Logarithmic];
        [Reactive] public MusicSpeedCurveTypes SelectedSpeedCurve { get; set; } = MusicSpeedCurveTypes.Linear;
        [Reactive] public List<SeekSpeed> SeekSpeedOptions { get; set; } = [SeekSpeed.Accurate, SeekSpeed.Insane];
        [Reactive] public SeekSpeed SelectedSeekSpeed { get; set; } = SeekSpeed.Accurate;
        [Reactive] public double MinSpeed { get; set; } = MusicConstants.Linear_Speed_Min;
        [Reactive] public double MaxSpeed { get; set; } = MusicConstants.Linear_Speed_Max;
        [Reactive] public bool Voice1Enabled { get; set; } = true;
        [Reactive] public bool Voice2Enabled { get; set; } = true;
        [Reactive] public bool Voice3Enabled { get; set; } = true;
        [Reactive] public bool RepeatModeEnabled { get; set; }
        [Reactive] public string SelectedScope { get; set; } = StorageScope.Storage.ToDescription();
        [Reactive]
        public List<string> ScopeOptions { get; set; } = Enum
            .GetValues(typeof(StorageScope))
            .Cast<StorageScope>().ToList()
            .Select(e => e.ToDescription())
            .ToList();
        [Reactive] public TimeProgressViewModel? Progress { get; set; } = null;
        [Reactive] public double ProgressSliderPercentage { get; set; } = 0.0;
        [Reactive] public int CurrentSubtuneIndex { get; set; }
        [Reactive] public bool SubtuneNextButtonEnabled { get; set; }
        [Reactive] public bool SubtunePreviousButtonEnabled { get; set; }
        [ObservableAsProperty] public bool SubtunesEnabled { get; }
        [ObservableAsProperty] public bool IsSong { get; }
        [ObservableAsProperty] public List<int> SubtuneNumberList { get; }
        [ObservableAsProperty] public bool ShowTitleOnly { get; }
        [ObservableAsProperty] public string StorageScopePath { get; }
        [ObservableAsProperty] public ILaunchableItem? File { get; }        
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
        public ReactiveCommand<Unit, Unit> ToggleRepeatCommand { get; set; }
        public ReactiveCommand<Unit, Unit> ToggleTimedPlay { get; set; }
        public ReactiveCommand<Unit, Unit> ToggleFavoriteCommand { get; set; }
        public ReactiveCommand<Unit, Unit> ShareCommand { get; set; }
        public ReactiveCommand<Unit, Unit> NavigateToFileDirCommand { get; set; }
        public ReactiveCommand<Unit, Unit> NextSubtuneCommand { get; set; }
        public ReactiveCommand<Unit, Unit> PreviousSubtuneCommand { get; set; }
        public ReactiveCommand<double, Unit> TrackTimeChangedCommand { get; set; }
        public ReactiveCommand<Unit, Unit> PauseTimerCommand { get; set; }
        public ReactiveCommand<Unit, Unit> PlaylistCommand { get; set; }

        private readonly IAlertService _alert;
        private IProgressTimer? _timer;
        private readonly Func<double, MusicSpeedCurveTypes, Task> _changeSpeed;
        private IDisposable? _timerCompleteSubscription;
        private IDisposable? _fastForwardTimerSubscription;
        private IDisposable? _currentTimeSubscription;
        private double _previousRawSpeed = 0;
        private SongItem? _currentSong;
        private MusicSpeedCurveTypes _previousSpeedCurve = MusicSpeedCurveTypes.Linear;
        private bool _muteFastForward;
        private bool _muteRandomSeek;
        private Func<bool, bool, bool, Task> _mute;
        private Func<Task> _restartSong;
        private Func<int, Task> _restartSubtune;
        private Func<Task> _playNext;
        private Func<Task> _togglePlay;
        private bool _midiTrackSeekInProgress;
        private TimeSpan? _currentSeekTargetTime;
        private double _originalSpeed = 0;
        private double _nudgeOffset = 0;
        private TeensySettings _settings;

        public PlayToolbarViewModel(
            IObservable<ILaunchableItem> file,
            IObservable<LaunchItemState> playState,
            IObservable<bool> timedPlayEnabled,
            IObservable<bool> muteFastForward,
            IObservable<bool> muteRandomSeek,
            IObservable<StorageScope> storageScope,
            IObservable<string> storageScopePath,
            IProgressTimer? timer,
            Func<Unit> toggleMode,
            Func<Task> togglePlay,
            Func<Task> playPrevious,
            Func<Task> playNext,
            Func<Task> restartSong,
            Func<int, Task> restartSubtune,
            Func<int, Task> playSubtune,
            Func<ILaunchableItem, Task> saveFav,
            Func<ILaunchableItem, Task> removeFav,
            Func<string, Task> loadDirectory,
            Func<double, MusicSpeedCurveTypes, Task> changeSpeed,
            Func<bool, bool, bool, Task> mute,
            Action<StorageScope> setScope,
            IAlertService alert, 
            IMidiService midiService,
            ISettingsService settingsService,
            IPlaylistDialogService playlist)
        {
            _timer = timer;
            _changeSpeed = changeSpeed;
            _alert = alert;
            _mute = mute;
            _togglePlay = togglePlay;
            _restartSong = restartSong;
            _restartSubtune = restartSubtune;
            _playNext = playNext;
            _settings = settingsService.GetSettings();

            settingsService.Settings.Subscribe(s => 
            {
                _settings = s;
            });

            muteFastForward.Subscribe(enabled => _muteFastForward = enabled);
            muteRandomSeek.Subscribe(enabled => _muteRandomSeek = enabled);

            var currentFile = file.Where(item => item is not null);

            var showReleaseInfo = currentFile
                .Select(item => !string.IsNullOrWhiteSpace(item.ReleaseInfo));

            var showCreatorInfo = currentFile
                .Select(item => !string.IsNullOrWhiteSpace(item.Creator));

            currentFile.Subscribe(async _ => await DisableFastForward());

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

            song.Subscribe(s => _currentSong = s);

            song.Select(s => s.SubtuneLengths.Count > 1)
                .ToPropertyEx(this, vm => vm.SubtunesEnabled);

            file.Select(f => f is SongItem)
                .ToPropertyEx(this, vm => vm.IsSong);

            file.Select(f => f is SongItem)
                .Subscribe(isSong =>
                {
                    SetSpeedEnabled = isSong;

                    if (RepeatModeEnabled) return;

                    Voice1Enabled = true;
                    Voice2Enabled = true;
                    Voice3Enabled = true;
                });

            song.Select(s => s.StartSubtuneNum)
                .Subscribe(startIndex =>
                {
                    CurrentSubtuneIndex = startIndex;
                });

            song.Select(s => s.SubtuneLengths.Select((_, i) => i + 1).ToList())
                .Where(lengths => lengths.Count > 1)
                .ToPropertyEx(this, vm => vm.SubtuneNumberList);

            song.Subscribe(StartSong);

            gameOrImage.Subscribe(item =>
            {
                RepeatModeEnabled = false;
                RawSpeedValue = 0;
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
                    RepeatModeEnabled = false;
                    TimedPlayButtonEnabled = true;
                    TimedPlayComboBoxEnabled = false;
                    ProgressEnabled = false;
                    _timer?.PauseTimer();
                });

            hexItem.Subscribe(item =>
            {
                RawSpeedValue = 0;
                RepeatModeEnabled = false;
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

            TogglePlayCommand = ReactiveCommand.CreateFromTask(_ => HandleTogglePlay());

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

            FastForwardCommand = ReactiveCommand.CreateFromTask(HandleFastForward);

            NextCommand = ReactiveCommand.CreateFromTask(playNext);

            PreviousCommand = ReactiveCommand.CreateFromTask(playPrevious);
            ToggleShuffleCommand = ReactiveCommand.Create(toggleMode);

            ToggleRepeatCommand = ReactiveCommand.Create<Unit, Unit>(_ => 
            {
                RepeatModeEnabled = !RepeatModeEnabled;
                return Unit.Default;
            });
            ToggleFavoriteCommand = ReactiveCommand.CreateFromTask(_ => 
            {
                if (File!.IsFavorite) 
                {
                    return removeFav(File!);                    
                }
                return saveFav(File!);
            });

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

            PauseTimerCommand = ReactiveCommand.Create(_timer!.PauseTimer);

            TrackTimeChangedCommand = ReactiveCommand.CreateFromTask<double>(async targetPercent =>
            {
                if (Progress is null) return;

                TimeSpan targetTime = Progress!.TotalTimeSpan.GetTimeFromPercent(targetPercent);

                await HandleSeek(restartSong, playSubtune, targetTime);
            });

            PlaylistCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await playlist.ShowDialog(File!);
            });

            this.WhenAnyValue(x => x.SelectedSpeedCurve)
                .Skip(1)
                .Where(_ => IsSong)
                .Subscribe(selectedSpeedCurve =>
                {
                    _previousSpeedCurve = FastForwardInProgress
                        ? MusicSpeedCurveTypes.Linear
                        : SelectedSpeedCurve;

                    MinSpeed = selectedSpeedCurve == MusicSpeedCurveTypes.Linear
                        ? MusicConstants.Linear_Speed_Min
                        : MusicConstants.Log_Speed_Min;

                    MaxSpeed = selectedSpeedCurve == MusicSpeedCurveTypes.Linear
                        ? MusicConstants.Linear_Speed_Max
                        : MusicConstants.Log_Speed_Max;

                    RawSpeedValue = 0;
                });

            var speedChanges = this.WhenAnyValue(x => x.RawSpeedValue)                
                .Skip(1)
                .Where(_ => IsSong)
                .Do(rawSpeed =>
                {
                    ActualSpeedPercent = SelectedSpeedCurve == MusicSpeedCurveTypes.Linear
                        ? Math.Round(rawSpeed, 2)
                        : rawSpeed.GetLogPercentage();

                    _timer?.UpdateSpeed(ActualSpeedPercent);
                })
                .DistinctUntilChanged()
                .ObserveOn(TaskPoolScheduler.Default)
                .Subscribe(async rawSpeed =>
                {
                    await _changeSpeed(rawSpeed, SelectedSpeedCurve).ConfigureAwait(false);
                });

            this.WhenAnyValue(x => x.CurrentSubtuneIndex)
                .Skip(2)
                .Where(_ => File is SongItem item && item.SubtuneLengths.Count > 1)
                .Subscribe(async songAndIndex =>
                {
                    await DisableFastForward(true);
                    InitializeSubtuneProgress(CurrentSubtuneIndex, playNext);
                    ResetSubtuneButtonState();
                    await playSubtune(CurrentSubtuneIndex);
                });

            midiService.MidiEvents
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(_ => IsSong)
                .Where(e => e.DJEventType is DJEventType.CurrentSpeed)
                .Select(e =>
                {
                    var ccMapping = e.Mapping as CCMapping;

                    return ccMapping!.CCType switch
                    {
                        CCType.Absolute => e.GetAbsoluteValueDelta(MinSpeed, MaxSpeed, RawSpeedValue),
                        CCType.Relative1 => e.GetRelativeValue_TwosComplement(e.Mapping.Amount),
                        CCType.Relative2 => e.GetRelativeValue_BinaryOffset(e.Mapping.Amount),
                        _ => 0
                    };
                })
                .Subscribe(async delta =>
                {
                    if (FastForwardInProgress) await DisableFastForward(true);

                    if (SelectedSpeedCurve == MusicSpeedCurveTypes.Logarithmic)
                    {
                        RawSpeedValue = ClampSpeed(RawSpeedValue.GetNearestPercentValue(delta));
                        _previousRawSpeed = 0;
                        return;
                    }
                    var finalSpeed = ClampSpeed(RawSpeedValue + delta);

                    RawSpeedValue = finalSpeed;
                    _previousRawSpeed = 0;
                });

            midiService.MidiEvents
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(_ => IsSong)
                .Where(e => e.DJEventType is DJEventType.CurrentSpeedFine)
                .Select(e =>
                {
                    var ccMapping = e.Mapping as CCMapping;

                    return ccMapping!.CCType switch
                    {
                        CCType.Absolute => e.GetAbsoluteValueDelta(MinSpeed, MaxSpeed, RawSpeedValue),
                        CCType.Relative1 => e.GetRelativeValue_TwosComplement(e.Mapping.Amount),
                        CCType.Relative2 => e.GetRelativeValue_BinaryOffset(e.Mapping.Amount),
                        _ => 0
                    };
                })
                .Subscribe(async delta =>
                {
                    if (FastForwardInProgress) await DisableFastForward(true);

                    if (SelectedSpeedCurve == MusicSpeedCurveTypes.Logarithmic)
                    {
                        RawSpeedValue = ClampSpeed(RawSpeedValue.GetNearestPercentValue(delta));
                        _previousRawSpeed = RawSpeedValue;
                        return;
                    }
                    var finalSpeed = ClampSpeed(RawSpeedValue + delta);

                    RawSpeedValue = finalSpeed;
                    _previousRawSpeed = RawSpeedValue;
                });

            midiService.MidiEvents
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(_ => IsSong)
                .Where(e => e.DJEventType is DJEventType.IncreaseCurrentSpeed or DJEventType.DecreaseCurrentSpeed or DJEventType.IncreaseCurrentSpeedFine or DJEventType.DecreaseCurrentSpeedFine)
                .Select(e => e.Mapping.Amount)
                .Subscribe(async delta =>
                {
                    if (FastForwardInProgress) await DisableFastForward(true);

                    if (SelectedSpeedCurve == MusicSpeedCurveTypes.Logarithmic)
                    {
                        RawSpeedValue = ClampSpeed(RawSpeedValue.GetNearestPercentValue(delta));
                        _previousRawSpeed = 0;
                        return;
                    }
                    var finalSpeed = ClampSpeed(RawSpeedValue + delta);

                    RawSpeedValue = finalSpeed;
                    _previousRawSpeed = 0;
                });

            midiService.MidiEvents
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(_ => IsSong)
                .Where(e => e.DJEventType is DJEventType.Voice1Kill or DJEventType.Voice2Kill or DJEventType.Voice3Kill)                
                .Subscribe(e =>
                {
                    if (e.DJEventType is DJEventType.Voice1Kill) Voice1Enabled = !Voice1Enabled;
                    if (e.DJEventType is DJEventType.Voice2Kill) Voice2Enabled = !Voice2Enabled;
                    if (e.DJEventType is DJEventType.Voice3Kill) Voice3Enabled = !Voice3Enabled;
                });

            midiService.MidiEvents
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(_ => IsSong)
                .Where(e => e.DJEventType is DJEventType.Voice1Toggle or DJEventType.Voice2Toggle or DJEventType.Voice3Toggle)
                .Subscribe(e =>
                {
                    if (e.DJEventType is DJEventType.Voice1Toggle) Voice1Enabled = !Voice1Enabled;
                    if (e.DJEventType is DJEventType.Voice2Toggle) Voice2Enabled = !Voice2Enabled;
                    if (e.DJEventType is DJEventType.Voice3Toggle) Voice3Enabled = !Voice3Enabled;
                });

            midiService.MidiEvents
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(_ => IsSong)
                .Subscribe(e => MessageBus.Current.SendMessage(e, MessageBusConstants.MidiCommandsReceived));

            midiService.MidiEvents
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(e => e.DJEventType is DJEventType.SpeedPlus50Toggle)
                .Where(_ => IsSong)
                .Do(async _ => await DisableFastForward(true))
                .Subscribe(_ => HandleIncrease50());

            midiService.MidiEvents
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(e => e.DJEventType is DJEventType.SpeedMinus50Toggle)
                .Where(_ => IsSong)
                .Do(async _ => await DisableFastForward(true))
                .Subscribe(e => HandleDecrease50());

            midiService.MidiEvents
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(e => e.DJEventType is DJEventType.HomeSpeedToggle)
                .Where(_ => IsSong)
                .Do(async _ => await DisableFastForward(true))
                .Subscribe(e => HandleHomeSpeed());

            midiService.MidiEvents
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(e => e.DJEventType is DJEventType.NudgeForward)
                .Where(_ => IsSong)
                .Do(async _ => await DisableFastForward(true))
                .Subscribe(e => HandleNudge(e));

            midiService.MidiEvents
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(e => e.DJEventType is DJEventType.NudgeBackward)
                .Where(_ => IsSong)
                .Do(async _ => await DisableFastForward(true))
                .Subscribe(e => HandleNudge(e));

            midiService.MidiEvents
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(_ => IsSong)
                .Where(e => e.DJEventType is DJEventType.Seek)                
                .Where(e => 
                {
                    var ccMapping = e.Mapping as CCMapping;
                    return ccMapping!.CCType is CCType.Relative1 or CCType.Relative2;
                })
                .Where(_ => Progress is not null)
                .Select(e =>
                {
                    var ccMapping = e.Mapping as CCMapping;

                    return ccMapping!.CCType switch
                    {
                        CCType.Absolute => e.GetAbsoluteValueDelta(0, 1, Progress!.CurrentSpan / _currentSong!.PlayLength),
                        CCType.Relative1 => e.GetRelativeValue_TwosComplement(e.Mapping.Amount),
                        CCType.Relative2 => e.GetRelativeValue_BinaryOffset(e.Mapping.Amount),
                        _ => 0
                    };
                })
                .Do(delta =>
                {
                    _midiTrackSeekInProgress = true;
                    var newProgressValue = ProgressSliderPercentage + delta;

                    if (newProgressValue > 1) //greater than 100% song length, wrap around.
                    {
                        ProgressSliderPercentage = newProgressValue - ProgressSliderPercentage;
                        return;
                    }
                    ProgressSliderPercentage = newProgressValue;
                })
                .Throttle(TimeSpan.FromMilliseconds(300))
                .Do(async _ => await DisableFastForward(true))
                .Do(_ => _midiTrackSeekInProgress = false)
                .Subscribe(async deltaPercent =>
                {
                    TimeSpan targetTime = Progress.TotalTimeSpan.GetTimeFromPercent(ProgressSliderPercentage);                    

                    await HandleSeek(restartSong, playSubtune, targetTime);
                });

            midiService.MidiEvents
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(_ => IsSong)
                .Where(e => e.DJEventType is DJEventType.Seek)
                .Where(e =>
                {
                    var ccMapping = e.Mapping as CCMapping;
                    return ccMapping!.CCType is CCType.Absolute;
                })
                .Where(_ => Progress is not null)
                .Do(e =>
                {   
                    _midiTrackSeekInProgress = true;
                    double percent = e.Value / 127.0;
                    ProgressSliderPercentage = percent;
                })
                .Throttle(TimeSpan.FromMilliseconds(300))
                .Do(async _ => await DisableFastForward(true))
                .Do(_ => _midiTrackSeekInProgress = false)
                .Subscribe(async _ =>
                {
                    TimeSpan targetTime = Progress.TotalTimeSpan.GetTimeFromPercent(ProgressSliderPercentage);

                    await HandleSeek(restartSong, playSubtune, targetTime);
                });

            midiService.MidiEvents
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(_ => IsSong)
                .Where(e => e.DJEventType is DJEventType.SeekForward or DJEventType.SeekBackward)
                .Where(_ => Progress is not null)
                .Select(e => e.Mapping.Amount)
                .Do(delta =>
                {
                    _midiTrackSeekInProgress = true;
                    var newProgressValue = ProgressSliderPercentage + delta;

                    if (newProgressValue > 1) //greater than 100% song length, wrap around.
                    {
                        ProgressSliderPercentage = newProgressValue - ProgressSliderPercentage;
                        return;
                    }
                    ProgressSliderPercentage = newProgressValue;
                })
                .Throttle(TimeSpan.FromMilliseconds(300))
                .Do(async _ => await DisableFastForward(true))
                .Do(_ => _midiTrackSeekInProgress = false)
                .Subscribe(async deltaPercent =>
                {
                    TimeSpan targetTime = Progress.TotalTimeSpan.GetTimeFromPercent(ProgressSliderPercentage);

                    await HandleSeek(restartSong, playSubtune, targetTime);
                });


            midiService.MidiEvents
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(_ => IsSong)
                .Where(e => e.DJEventType is DJEventType.FastForward)
                .Subscribe(async e => await HandleFastForward());

            midiService.MidiEvents
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(e => e.DJEventType is DJEventType.PlayPause)
                .Subscribe(async e => await HandleTogglePlay());

            midiService.MidiEvents
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(e => e.DJEventType is DJEventType.HoldPause)                
                .Subscribe(async e => await HandleHoldPause());


            midiService.MidiEvents
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(e => e.DJEventType is DJEventType.Previous)
                .Subscribe(e => playPrevious());


            midiService.MidiEvents
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(e => e.DJEventType is DJEventType.Next)
                .Subscribe(e => playNext());

            midiService.MidiEvents
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(e => e.DJEventType is DJEventType.Mode)
                .Subscribe(e => toggleMode());

            midiService.MidiEvents
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(e => e.DJEventType is DJEventType.Restart)
                .Do(async _ => await DisableFastForward(true))
                .Subscribe(async e => await HandleRestart());

            MessageBus.Current.Listen<KeyboardShortcut>(MessageBusConstants.RestartKeyPressed)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(_ => IsSong)
                .Do(async _ => await DisableFastForward(true))
                .Subscribe(async e => await HandleRestart());

            MessageBus.Current.Listen<KeyboardShortcut>(MessageBusConstants.MediaPlayerKeyPressed)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(async key => 
                {
                    switch (key) 
                    {
                        case KeyboardShortcut.PlayPause:
                            await HandleTogglePlay();
                            break;
                        case KeyboardShortcut.Stop when StopButtonEnabled || PauseButtonEnabled:
                            await HandleTogglePlay();
                            break;
                        case KeyboardShortcut.PreviousTrack:
                            await playPrevious();
                            break;
                        case KeyboardShortcut.NextTrack:
                            await playNext();
                            break;
                    } 
                });

            MessageBus.Current.Listen<KeyboardShortcut>(MessageBusConstants.SidVoiceMuteKeyPressed)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(_ => IsSong is true)
                .Subscribe(key =>
                {
                    if (key == KeyboardShortcut.Voice1Toggle) Voice1Enabled = !Voice1Enabled;
                    if (key == KeyboardShortcut.Voice2Toggle) Voice2Enabled = !Voice2Enabled;
                    if (key == KeyboardShortcut.Voice3Toggle) Voice3Enabled = !Voice3Enabled;
                });

            MessageBus.Current.Listen<double>(MessageBusConstants.SidSpeedIncreaseKeyPressed)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(_ => IsSong is true)
                .Do(async _ => await DisableFastForward(true))
                .Subscribe(amt =>
                {
                    if(SelectedSpeedCurve == MusicSpeedCurveTypes.Logarithmic)
                    {
                        RawSpeedValue = ClampSpeed(RawSpeedValue.GetNearestPercentValue(amt));
                        _previousRawSpeed = RawSpeedValue;
                        return;
                    }

                    var finalSpeed = ClampSpeed(RawSpeedValue += amt);

                    RawSpeedValue = finalSpeed;
                    _previousRawSpeed = RawSpeedValue;
                });

            MessageBus.Current.Listen<double>(MessageBusConstants.SidSpeedDecreaseKeyPressed)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(_ => IsSong is true)
                .Do(async _ => await DisableFastForward(true))
                .Subscribe(amt =>
                {
                    if (SelectedSpeedCurve == MusicSpeedCurveTypes.Logarithmic)
                    {
                        RawSpeedValue = ClampSpeed(RawSpeedValue.GetNearestPercentValue(amt));
                        _previousRawSpeed = RawSpeedValue;
                        return;
                    }
                    var finalSpeed = ClampSpeed(RawSpeedValue += amt);
                    RawSpeedValue = finalSpeed;
                    _previousRawSpeed = RawSpeedValue;
                });

            MessageBus.Current.Listen<KeyboardShortcut>(MessageBusConstants.SidSpeedIncrease50KeyPressed)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(_ => IsSong is true)
                .Subscribe(_ =>
                {
                    HandleIncrease50();
                });

            MessageBus.Current.Listen<KeyboardShortcut>(MessageBusConstants.SidSpeedDecrease50KeyPressed)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(_ => IsSong is true)
                .Do(async _ => await DisableFastForward(true))
                .Subscribe(_ =>
                {
                    HandleDecrease50();
                });

            MessageBus.Current.Listen<KeyboardShortcut>(MessageBusConstants.SidSpeedHomeKeyPressed)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(_ => IsSong is true)
                .Do(async _ => await DisableFastForward(true))
                .Subscribe(_ =>
                {
                    HandleHomeSpeed();
                });

            MessageBus.Current.Listen<KeyboardShortcut>(MessageBusConstants.FastForwardKeyPressed)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(_ => IsSong is true)
                .Subscribe(async _ =>
                {
                   await HandleFastForward();
                });

            this.WhenAnyValue(vm => vm.Voice1Enabled, vm => vm.Voice2Enabled, vm => vm.Voice3Enabled)
                .Skip(1)
                .Where(_ => IsSong)
                .Buffer(TimeSpan.FromMilliseconds(50)) 
                .Where(buffer => buffer.Any())
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(async buffer => await mute(!Voice1Enabled, !Voice2Enabled, !Voice3Enabled));
        }

        private void HandleNudge(MidiEvent midiEvent) 
        {
            var offset = midiEvent.Mapping.Amount;

            if (midiEvent.MidiEventType is MidiEventType.NoteOn)
            {
                _nudgeOffset = offset;

                RawSpeedValue = SelectedSpeedCurve == MusicSpeedCurveTypes.Logarithmic
                    ? ClampSpeed(RawSpeedValue.GetNearestPercentValue(offset))
                    : ClampSpeed(RawSpeedValue + offset);
                return;
            }
            if (midiEvent.MidiEventType is MidiEventType.NoteOff)
            {
                RawSpeedValue -= _nudgeOffset;                
                _nudgeOffset = 0;
            }
        }

        private async Task HandleRestart()
        {
            if (_timer is null) return;

            if (PlayButtonEnabled)
            {
                await _togglePlay();
            }
            await _restartSubtune(CurrentSubtuneIndex);
            _timer.ResetTimer();
        }

        private void StartSong(SongItem item)
        {
            TimedPlayButtonEnabled = false;
            TimedPlayComboBoxEnabled = false;
            ProgressEnabled = true;
            RawSpeedValue = 0;
            _previousRawSpeed = RawSpeedValue;
            InitializeProgress(_playNext, item);
        }


        private async Task HandleSeek(Func<Task> restartSong, Func<int, Task> playSubtune, TimeSpan targetTime)
        {
            if (_currentSong is null || Progress is null || _timer is null) return;

            if (targetTime == _currentSeekTargetTime) return;

            if (TrackSeekInProgress)
            {
                await CancelSeek();
            }

            var isPaused = false;

            if (PlayButtonEnabled) 
            {
                isPaused = true;
            }
            PlayButtonEnabled = true;
            PauseButtonEnabled = false;

            try
            {
                var nearlyEndOfSong = targetTime >= Progress.TotalTimeSpan - TimeSpan.FromMilliseconds(500);  //The subtraction avoids a potential loop.

                if (targetTime < Progress!.CurrentSpan || nearlyEndOfSong)
                {
                    if (_currentSong.SubtuneLengths?.Count > 1)
                    {
                        await playSubtune(CurrentSubtuneIndex);
                    }
                    else 
                    {
                        await restartSong();
                    }
                    if (targetTime <= TimeSpan.Zero || nearlyEndOfSong)
                    {
                        _timer?.ResetTimer();
                        _midiTrackSeekInProgress = false;
                        TrackSeekInProgress = false;
                        return;
                    }
                    _timer?.ResetTimer();
                    await Task.Delay(500);
                }
                _currentSeekTargetTime = targetTime;

                if (_muteRandomSeek) await _mute(true, true, true);

                if (isPaused) 
                {
                    PlayButtonEnabled = false;
                    PauseButtonEnabled = true;
                    _timer?.ResumeTimer();
                    await _togglePlay();                    
                }

                var seekSpeed = SelectedSeekSpeed is SeekSpeed.Accurate
                ? MusicConstants.Log_Speed_Max_Accurate
                : MusicConstants.Log_Speed_Max;

                await _changeSpeed(seekSpeed, MusicSpeedCurveTypes.Logarithmic);
                _timer?.UpdateSpeed(seekSpeed.GetLogPercentage());
                _midiTrackSeekInProgress = false;
                TrackSeekInProgress = true;

                _fastForwardTimerSubscription?.Dispose();

                _fastForwardTimerSubscription = _timer?.CurrentTime.Subscribe(async currentTime =>
                {
                    try
                    {
                        if (currentTime >= _currentSeekTargetTime || _currentSeekTargetTime == null)   //TODO: Smell: checking for null here to prevent runaway timer.  Find out why _changeSpeed() threw an Ex to cause the null.
                        {
                            await CancelSeek();
                        }
                    }
                    catch (TeensyDjException)
                    {
                        await CancelSeek();
                        return;
                    }
                });
            }
            catch (TeensyDjException)
            {   
                await CancelSeek();
                return;
            }
        }

        private async Task CancelSeek()
        {
            try
            {
                _currentSeekTargetTime = null;
                _fastForwardTimerSubscription?.Dispose();

                var originalSpeed = SelectedSpeedCurve == MusicSpeedCurveTypes.Linear
                    ? Math.Round(RawSpeedValue, 2)
                    : RawSpeedValue.GetLogPercentage();

                _timer?.UpdateSpeed(_originalSpeed);
                RawSpeedValue = _originalSpeed;
                await _changeSpeed(_originalSpeed, SelectedSpeedCurve);
                TrackSeekInProgress = false;
                PlayButtonEnabled = false;
                PauseButtonEnabled = true;
                if (_muteFastForward) await _mute(false, false, false);
            }
            catch (Exception)
            {
                //swallow the exception.  Not much we can do.  This could be a problemmatic SID.
                _alert.Publish("This SID might be incompatible with DJ functions");
            }
        }

        private void HandleHomeSpeed()
        {
            if (_previousRawSpeed == RawSpeedValue)
            {
                RawSpeedValue = 0;
                return;
            }
            RawSpeedValue = _previousRawSpeed;
        }

        private void HandleDecrease50()
        {
            if (SelectedSpeedCurve == MusicSpeedCurveTypes.Logarithmic)
            {
                if (RawSpeedValue == _previousRawSpeed.GetNearestPercentValue(-50) && RawSpeedValue != 0) return;

                if (RawSpeedValue == _previousRawSpeed.GetNearestPercentValue(50))
                {
                    RawSpeedValue = _previousRawSpeed;
                    return;
                }
                RawSpeedValue = ClampSpeed(RawSpeedValue.GetNearestPercentValue(-50));
                return;
            }
            if (RawSpeedValue == _previousRawSpeed - 50 && RawSpeedValue != 0) return;

            if (RawSpeedValue == _previousRawSpeed + 50)
            {
                RawSpeedValue = _previousRawSpeed;
                return;
            }
            _previousRawSpeed = RawSpeedValue;

            RawSpeedValue = ClampSpeed(RawSpeedValue - 50);
        }

        private void HandleIncrease50()
        {
            if (SelectedSpeedCurve == MusicSpeedCurveTypes.Logarithmic)
            {
                if (RawSpeedValue == _previousRawSpeed.GetNearestPercentValue(50) && RawSpeedValue != 0) return;

                if (RawSpeedValue == _previousRawSpeed.GetNearestPercentValue(-50))
                {
                    RawSpeedValue = _previousRawSpeed;
                    return;
                }
                RawSpeedValue = ClampSpeed(RawSpeedValue.GetNearestPercentValue(50));
                return;
            }
            if (RawSpeedValue == _previousRawSpeed + 50 && RawSpeedValue != 0) return;

            if (RawSpeedValue == _previousRawSpeed - 50)
            {
                RawSpeedValue = _previousRawSpeed;
                return;
            }
            _previousRawSpeed = RawSpeedValue;

            RawSpeedValue = ClampSpeed(RawSpeedValue + 50);
        }

        private async Task HandleHoldPause()
        {

            if (TrackSeekInProgress)
            {
                await CancelSeek();
                return;
            }
            if (PlayButtonEnabled) _timer?.ResumeTimer();
            if (PauseButtonEnabled) _timer?.PauseTimer();

            await _togglePlay();

            return;
        }

        private async Task HandleTogglePlay()
        {
            if (FastForwardInProgress)
            {
                await DisableFastForward(true);
                return;
            }
            if (TrackSeekInProgress)
            {
                await CancelSeek();
                return;
            }
            if (PlayButtonEnabled) _timer?.ResumeTimer();
            if (PauseButtonEnabled) _timer?.PauseTimer();
            
            await _togglePlay();

            return;
        }

        private double ClampSpeed(double speed)
        {
            var (min, max) = SelectedSpeedCurve == MusicSpeedCurveTypes.Linear
                ? (MusicConstants.Linear_Speed_Min, MusicConstants.Linear_Speed_Max)
                : (MusicConstants.Log_Speed_Min, MusicConstants.Log_Speed_Max);

            return Math.Clamp(speed, min, max);
        }


        private async Task  HandleFastForward()
        {
            switch (FastForwardSpeed)
            {
                case FastForwardSpeed.Off:
                    PauseButtonEnabled = false;
                    PlayButtonEnabled = true;
                    FastForwardInProgress = true;
                    SetSpeedEnabled = false;
                    FastForwardSpeed = FastForwardSpeed.Medium;
                    _previousRawSpeed = RawSpeedValue;
                    _previousSpeedCurve = SelectedSpeedCurve;
                    SelectedSpeedCurve = MusicSpeedCurveTypes.Logarithmic;

                    if (_muteFastForward) await _mute(true, true, true);
                    
                    RawSpeedValue = 25;                    
                    return;

                case FastForwardSpeed.Medium:
                    FastForwardSpeed = FastForwardSpeed.MediumFast;

                    if (_muteFastForward) await _mute(true, true, true);
                    
                    RawSpeedValue = 50;                    
                    return;

                case FastForwardSpeed.MediumFast:
                    FastForwardSpeed = FastForwardSpeed.Fast;

                    if (_muteFastForward) await _mute(true, true, true);

                    RawSpeedValue = 75;                    
                    return;

                case FastForwardSpeed.Fast:
                    FastForwardSpeed = FastForwardSpeed.Hyper;

                    if (_muteFastForward) await _mute(true, true, true);

                    RawSpeedValue = SelectedSpeedCurve == MusicSpeedCurveTypes.Linear ? MusicConstants.Linear_Speed_Max : MusicConstants.Log_Speed_Max_Accurate;                    
                    return;

                case FastForwardSpeed.Hyper:
                    await DisableFastForward(true);
                    return;
            }
        }

        private async Task  DisableFastForward(bool resumePreviousSpeed = false)
        {
            if(FastForwardInProgress is false) return;

            PlayButtonEnabled = false;
            PauseButtonEnabled = true;
            FastForwardInProgress = false;
            SetSpeedEnabled = true;
            FastForwardSpeed = FastForwardSpeed.Off;
            SelectedSpeedCurve = _previousSpeedCurve;

            if (_muteFastForward) 
            {
                await _mute(false, false, false);
                await Task.Delay(100);
            }
            
            RawSpeedValue = resumePreviousSpeed ? _previousRawSpeed : 0;
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

        private void StartTimerObservables(TimeSpan timespan, Func<Task> onNext, bool startNewTimer = true) 
        {
            if (_timer == null) return;

            _timerCompleteSubscription?.Dispose();
            _currentTimeSubscription?.Dispose();

            if(startNewTimer) _timer?.StartNewTimer(timespan);

            _currentTimeSubscription = _timer?.CurrentTime
                .Select(t => new TimeProgressViewModel(timespan, t))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(t => 
                {
                    Progress = t;

                    if (_midiTrackSeekInProgress is false)
                    {
                        ProgressSliderPercentage = t.Percentage;
                    }
                });

            _timerCompleteSubscription = _timer?.TimerComplete
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(async _ =>
                {
                    if (RepeatModeEnabled)
                    {
                        await HandleRepeatMode();
                        return;
                    }                    
                    await onNext();
                });
        }

        private async Task HandleRepeatMode()
        {
            if (CurrentSubtuneIndex > 1)
            {
                InitializeSubtuneProgress(CurrentSubtuneIndex, _playNext);
                return;
            }
            else 
            {
                InitializeProgress(_playNext, _currentSong);
            }
            if (RawSpeedValue != 0)
            {
                await Task.Delay(200);
                await _changeSpeed(RawSpeedValue, SelectedSpeedCurve);
            }        
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
