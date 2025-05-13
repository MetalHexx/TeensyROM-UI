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
using TeensyRom.Ui.Features.Discover.State;
using TeensyRom.Ui.Core.Progress;
using System.Reactive.Concurrency;
using TeensyRom.Core.Music;
using TeensyRom.Ui.Services;
using TeensyRom.Ui.Main;
using TeensyRom.Core.Midi;
using TeensyRom.Core.Settings;
using TeensyRom.Ui.Controls.Playlist;
using TeensyRom.Ui.Services.Process;
using System.Reactive.Subjects;
using System.Diagnostics;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Entities.Midi;
using TeensyRom.Core.Commands.PlaySubtune;
using TeensyRom.Core.Commands.MuteSidVoices;
using TeensyRom.Core.Serial.Commands.Composite.StartSeek;

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
        [Reactive] public bool ProgressSeparatorEnabled { get; set; } = false;
        [Reactive] public bool AdvancedEnabled { get; set; } = false;
        [Reactive] public string TimerSeconds { get; set; } = "3m";
        [Reactive] public List<string> TimerOptions { get; set; } = ["5s", "10s", "15s", "30s", "1m", "3m", "5m", "10m", "15m", "30m"];
        [Reactive] public bool FastForwardInProgress { get; set; }
        [Reactive] public bool TrackSeekInProgress { get; set; }
        [Reactive] public FastForwardSpeed FastForwardSpeed { get; set; } = FastForwardSpeed.Off;
        [Reactive] public bool SetSpeedEnabled { get; set; }
        [Reactive] public double RawSpeedValue { get; set; }
        private BehaviorSubject<double> _rawSpeedValue { get; set; } = new(0);
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
        public ReactiveCommand<Unit, Unit> SaveSongSettingsCommand { get; set; }
        public ReactiveCommand<Unit, Unit> SendSongCommand { get; set; }

        private readonly IAlertService _alert;
        private readonly ICrossProcessService _crossProcess;
        private IProgressTimer? _timer;
        private readonly Func<double, MusicSpeedCurveTypes, Task> _changeSpeed;
        private IDisposable? _timerCompleteSubscription;
        private IDisposable? _fastForwardTimerSubscription;
        private IDisposable? _currentTimeSubscription;
        private double _previousRawSpeed = 0;
        private bool _forceSpeedChange = false;
        private bool _shouldEndFastForward = false;
        private SongItem? _currentSong;
        private MusicSpeedCurveTypes _previousSpeedCurve = MusicSpeedCurveTypes.Linear;
        private bool _muteFastForward;
        private bool _muteRandomSeek;
        private readonly Func<VoiceState, VoiceState, VoiceState, Task> _mute;
        private readonly Func<int, bool, bool, double, SeekDirection, Task> _startSeek;
        private readonly Func<bool, double, MusicSpeedCurveTypes, Task> _endSeek;
        private readonly Func<bool, bool, double, Task> _fastForward;
        private readonly Func<bool, double, MusicSpeedCurveTypes, Task> _endFastForward;
        private readonly Func<Task> _restartSong;
        private readonly Func<int, Task<PlaySubtuneResult?>> _restartSubtune;
        private readonly Func<int, Task<PlaySubtuneResult?>> _playSubtune;
        private readonly Func<Task> _playNext;
        private readonly Func<Task> _togglePlay;
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
            Func<int, Task<PlaySubtuneResult?>> restartSubtune,
            Func<int, Task<PlaySubtuneResult?>> playSubtune,
            Func<ILaunchableItem, Task> saveFav,
            Func<ILaunchableItem, Task> removeFav,
            Func<string, Task> loadDirectory,
            Func<double, MusicSpeedCurveTypes, Task> changeSpeed,
            Func<VoiceState, VoiceState, VoiceState, Task> mute,
            Func<int, bool, bool, double, SeekDirection, Task> startSeek,
            Func<bool, double, MusicSpeedCurveTypes, Task> endSeek,
            Func<bool, bool, double, Task> fastForward,
            Func<bool, double, MusicSpeedCurveTypes, Task> endFastForward,
            Action<StorageScope> setScope,
            IAlertService alert, 
            IMidiService midiService,
            ISettingsService settingsService,
            IPlaylistDialogService playlist,
            ICrossProcessService crossProcess)
        {
            _timer = timer;
            _changeSpeed = changeSpeed;
            _alert = alert;
            _crossProcess = crossProcess;
            _mute = mute;
            _startSeek = startSeek;
            _endSeek = endSeek;
            _fastForward = fastForward;
            _endFastForward = endFastForward;
            _togglePlay = togglePlay;
            _restartSong = restartSong;
            _restartSubtune = restartSubtune;
            _playSubtune = playSubtune;
            _playNext = playNext;
            _settings = settingsService.GetSettings();

            settingsService.Settings.Subscribe(s => 
            {
                _settings = s;
            });

            settingsService.Settings
                .Where(s => s is not null)
                .Take(1)
                .Subscribe(s => RepeatModeEnabled = s.RepeatModeOnStartup);

            muteFastForward.Subscribe(enabled => _muteFastForward = enabled);
            muteRandomSeek.Subscribe(enabled => _muteRandomSeek = enabled);

            var currentFile = file.Where(item => item is not null);

            var showReleaseInfo = currentFile
                .Select(item => !string.IsNullOrWhiteSpace(item.ReleaseInfo));

            var showCreatorInfo = currentFile
                .Select(item => !string.IsNullOrWhiteSpace(item.Creator));

            currentFile.Subscribe(async _ => await CancelSpeed());

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
                    if (scopeAndPath.path == StorageHelper.Remote_Path_Root) return scopeAndPath;

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
                .Do(_ => 
                {
                    if (TrackSeekInProgress) return;

                    PlayButtonEnabled = false;
                });

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
                    if (TrackSeekInProgress) return;

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

            song
                .WithLatestFrom(playState, (song, state) => (song, state))
                .Where(pair => pair.state.PlayState is PlayState.Playing)
                .Select(pair => pair.song)
                .Subscribe(async s => await StartSong(s));

            gameOrImage.Subscribe(item =>
            {
                RepeatModeEnabled = false;
                SetSpeed(0);
            });

            gameOrImage
                .Where(_ => TimedPlayEnabled)
                .Subscribe(item =>
                {   
                    TimedPlayButtonEnabled = true;
                    TimedPlayComboBoxEnabled = true;
                    ProgressEnabled = true;
                    InitializeProgress(item);
                });

            gameOrImage
                .Where(_ => !TimedPlayEnabled)
                .Subscribe(item =>
                {
                    RepeatModeEnabled = false;
                    TimedPlayButtonEnabled = true;
                    TimedPlayComboBoxEnabled = false;
                    ProgressEnabled = false;
                    _currentTimeSubscription?.Dispose();
                    _currentTimeSubscription = null;
                });

            hexItem.Subscribe(item =>
            {
                SetSpeed(0);
                RepeatModeEnabled = false;
                TimedPlayButtonEnabled = false;
                TimedPlayComboBoxEnabled = false;
                ProgressEnabled = false;
                _currentTimeSubscription?.Dispose();
                _currentTimeSubscription = null;
            });

            this.WhenAnyValue(x => x.TimerSeconds)
                .Where(_ => TimedPlayEnabled)
                .Subscribe(timeSpan =>
                {
                    InitializeProgress(File);
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
                    InitializeProgress(File);
                });

            TogglePlayCommand = ReactiveCommand.CreateFromTask(_ => HandleTogglePlay());

            ToggleTimedPlay = ReactiveCommand.Create<Unit>(_ =>
            {
                TimedPlayEnabled = !TimedPlayEnabled;

                if (TimedPlayEnabled)
                {
                    TimedPlayComboBoxEnabled = true;
                    ProgressEnabled = true;
                    InitializeProgress(File);
                    return;
                }
                TimedPlayComboBoxEnabled = false;
                ProgressEnabled = false;
                _timer?.PauseTimer();
            });

            FastForwardCommand = ReactiveCommand.CreateFromTask(HandleFastForward);

            NextCommand = ReactiveCommand.CreateFromTask(_playNext);

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

                await HandleSeek(targetTime);
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
                });  
                
            _rawSpeedValue
                .Where(_ => IsSong)
                .Do(rawSpeed =>
                {
                    ActualSpeedPercent = SelectedSpeedCurve == MusicSpeedCurveTypes.Linear
                        ? Math.Round(rawSpeed, 2)
                        : rawSpeed.GetLogPercentage();

                    _timer?.UpdateSpeed(ActualSpeedPercent);
                })
                .ObserveOn(TaskPoolScheduler.Default)
                .Scan
                (
                   (Previous: (double?)null, Current: (double?)null),
                   (acc, next) => (acc.Current, next)
                )    
                .Where(t =>
                {
                    var prev = t.Previous;
                    var next = t.Current;

                    if (_forceSpeedChange)
                    {
                        _forceSpeedChange = false;
                        return true;
                    }
                    return prev != next;
                })
                .Select(t => t.Current!.Value)                
                .Subscribe(async rawSpeed =>
                {
                    Debug.WriteLine($"Raw Speed: {rawSpeed}");
                    
                    if (FastForwardInProgress) 
                    {
                        await _fastForward(false, _muteFastForward, rawSpeed);
                        return;
                    }
                    if (_shouldEndFastForward is true) 
                    {
                        await _endFastForward(_muteFastForward, rawSpeed, SelectedSpeedCurve);
                        _shouldEndFastForward = false;
                        return;
                    }
                    await _changeSpeed(rawSpeed, SelectedSpeedCurve).ConfigureAwait(false);
                });

            this.WhenAnyValue(x => x.CurrentSubtuneIndex)
                .Skip(2)
                .Where(_ => File is SongItem item && item.SubtuneLengths.Count > 1)
                .Subscribe(async songAndIndex =>
                {
                    await CancelSpeed(true);
                    InitializeSubtuneProgress(CurrentSubtuneIndex);
                    ResetSubtuneButtonState();
                    await playSubtune(CurrentSubtuneIndex);
                });

            midiService.MidiEvents
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(_ => IsSong)
                .Where(e => e.DJEventType is DJEventType.CurrentSpeed)
                .Do(async _ => await CancelSpeed(true))
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

                    if (SelectedSpeedCurve == MusicSpeedCurveTypes.Logarithmic)
                    {
                        SetSpeed(ClampSpeed(_rawSpeedValue.Value.GetNearestPercentValue(delta)));
                        _previousRawSpeed = 0;
                        return;
                    }
                    var finalSpeed = ClampSpeed(_rawSpeedValue.Value + delta);

                    SetSpeed(finalSpeed);
                    _previousRawSpeed = 0;
                });

            midiService.MidiEvents
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(_ => IsSong)
                .Where(e => e.DJEventType is DJEventType.CurrentSpeedFine)
                .Do(async _ => await CancelSpeed(true))
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
                    if (SelectedSpeedCurve == MusicSpeedCurveTypes.Logarithmic)
                    {
                        SetSpeed(ClampSpeed(_rawSpeedValue.Value.GetNearestPercentValue(delta)));
                        _previousRawSpeed = _rawSpeedValue.Value;
                        return;
                    }
                    var finalSpeed = ClampSpeed(_rawSpeedValue.Value + delta);

                    SetSpeed(finalSpeed);
                    _previousRawSpeed = _rawSpeedValue.Value;
                });

            midiService.MidiEvents
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(_ => IsSong)
                .Where(e => e.DJEventType is DJEventType.IncreaseCurrentSpeed or DJEventType.DecreaseCurrentSpeed or DJEventType.IncreaseCurrentSpeedFine or DJEventType.DecreaseCurrentSpeedFine)
                .Do(async _ => await CancelSpeed(true))
                .Select(e => e.Mapping.Amount)
                .Subscribe(async delta =>
                {

                    if (SelectedSpeedCurve == MusicSpeedCurveTypes.Logarithmic)
                    {
                        SetSpeed(ClampSpeed(_rawSpeedValue.Value.GetNearestPercentValue(delta)));
                        _previousRawSpeed = 0;
                        return;
                    }
                    var finalSpeed = ClampSpeed(_rawSpeedValue.Value + delta);

                    SetSpeed(finalSpeed);
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
                .Do(async _ => await CancelSpeed(true))
                .Subscribe(_ => HandleIncrease50());

            midiService.MidiEvents
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(e => e.DJEventType is DJEventType.SpeedMinus50Toggle)
                .Where(_ => IsSong)
                .Do(async _ => await CancelSpeed(true))
                .Subscribe(e => HandleDecrease50());

            midiService.MidiEvents
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(e => e.DJEventType is DJEventType.HomeSpeedToggle)
                .Where(_ => IsSong)
                .Do(async _ => await CancelSpeed(true))
                .Subscribe(e => HandleHomeSpeed());

            midiService.MidiEvents
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(e => e.DJEventType is DJEventType.NudgeForward)
                .Where(_ => IsSong)
                .Do(async _ => await CancelSpeed(true))
                .Subscribe(e => HandleNudge(e));

            midiService.MidiEvents
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(e => e.DJEventType is DJEventType.NudgeBackward)
                .Where(_ => IsSong)
                .Do(async _ => await CancelSpeed(true))
                .Subscribe(e => HandleNudge(e));

            //For Relative CC
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
                .Throttle(TimeSpan.FromMilliseconds(200))
                .Do(_ => _midiTrackSeekInProgress = false)
                .Subscribe(async deltaPercent =>
                {
                    TimeSpan targetTime = Progress.TotalTimeSpan.GetTimeFromPercent(ProgressSliderPercentage);                    

                    await HandleSeek(targetTime);
                });

            //For absolute CC
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
                .Throttle(TimeSpan.FromMilliseconds(200))
                .Do(_ => _midiTrackSeekInProgress = false)
                .Subscribe(async _ =>
                {
                    TimeSpan targetTime = Progress.TotalTimeSpan.GetTimeFromPercent(ProgressSliderPercentage);

                    await HandleSeek(targetTime);
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
                .Do(async _ => await CancelSpeed(true))
                .Subscribe(async e => await HandleHoldPause());


            midiService.MidiEvents
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(e => e.DJEventType is DJEventType.Previous)
                .Do(async _ => await CancelSpeed(true))
                .Subscribe(e => playPrevious());


            midiService.MidiEvents
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(e => e.DJEventType is DJEventType.Next)
                .Do(async _ => await CancelSpeed(true))
                .Subscribe(e => _playNext());

            midiService.MidiEvents
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(e => e.DJEventType is DJEventType.Mode)
                .Subscribe(e => toggleMode());

            midiService.MidiEvents
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(e => e.DJEventType is DJEventType.Restart)                
                .Subscribe(async e => await HandleRestart());

            midiService.MidiEvents
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(e => e.DJEventType is DJEventType.CrossLaunch)
                .Subscribe(async _ => await HandleCrossLaunch());

            midiService.MidiEvents
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(e => e.DJEventType is DJEventType.SaveSongSettings)
                .Subscribe(async _ => await HandleSaveSongSettings());

            MessageBus.Current.Listen<KeyboardShortcut>(MessageBusConstants.RestartKeyPressed)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(_ => IsSong)
                .Do(async _ => await CancelSpeed(true))
                .Subscribe(async e => await HandleRestart());

            MessageBus.Current.Listen<KeyboardShortcut>(MessageBusConstants.MediaPlayerKeyPressed)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Do(async _ => await CancelSpeed(true))
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
                            await _playNext();
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
                .Do(async _ => await CancelSpeed(true))
                .Subscribe(amt =>
                {
                    if(SelectedSpeedCurve == MusicSpeedCurveTypes.Logarithmic)
                    {
                        SetSpeed(ClampSpeed(_rawSpeedValue.Value.GetNearestPercentValue(amt)));
                        _previousRawSpeed = _rawSpeedValue.Value;
                        return;
                    }
                    var newSpeed = _rawSpeedValue.Value + amt;
                    var finalSpeed = ClampSpeed(newSpeed);

                    SetSpeed(finalSpeed);
                    _previousRawSpeed = _rawSpeedValue.Value;
                });

            MessageBus.Current.Listen<double>(MessageBusConstants.SidSpeedDecreaseKeyPressed)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(_ => IsSong is true)
                .Do(async _ => await CancelSpeed(true))
                .Subscribe(amt =>
                {
                    if (SelectedSpeedCurve == MusicSpeedCurveTypes.Logarithmic)
                    {
                        SetSpeed(ClampSpeed(_rawSpeedValue.Value.GetNearestPercentValue(amt)));
                        _previousRawSpeed = _rawSpeedValue.Value;
                        return;
                    }
                    var newSpeed = _rawSpeedValue.Value + amt;
                    var finalSpeed = ClampSpeed(newSpeed);
                    SetSpeed(finalSpeed);
                    _previousRawSpeed = _rawSpeedValue.Value;
                });

            MessageBus.Current.Listen<KeyboardShortcut>(MessageBusConstants.SidSpeedIncrease50KeyPressed)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(_ => IsSong is true)
                .Do(async _ => await CancelSpeed(true))
                .Subscribe(_ =>
                {
                    HandleIncrease50();
                });

            MessageBus.Current.Listen<KeyboardShortcut>(MessageBusConstants.SidSpeedDecrease50KeyPressed)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(_ => IsSong is true)
                .Do(async _ => await CancelSpeed(true))
                .Subscribe(_ =>
                {
                    HandleDecrease50();
                });

            MessageBus.Current.Listen<KeyboardShortcut>(MessageBusConstants.SidSpeedHomeKeyPressed)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(_ => IsSong is true)
                .Do(async _ => await CancelSpeed(true))
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
                .Subscribe(async buffer => await mute(
                    Voice1Enabled is true ? VoiceState.Enabled : VoiceState.Disabled, 
                    Voice2Enabled is true ? VoiceState.Enabled : VoiceState.Disabled, 
                    Voice3Enabled is true ? VoiceState.Enabled : VoiceState.Disabled));

            SaveSongSettingsCommand = ReactiveCommand.CreateFromTask(HandleSaveSongSettings);

            SendSongCommand = ReactiveCommand.CreateFromTask(HandleCrossLaunch);
        }

        private async Task HandleSaveSongSettings()
        {
            if (File is not SongItem song) return;

            song.Custom = new PlaylistItem
            {
                FilePath = song.Path,
                DefaultSpeed = RawSpeedValue,
                DefaultSpeedCurve = SelectedSpeedCurve,
            };
            await _crossProcess.UpsertFile(song);
        }

        private async Task HandleCrossLaunch()
        {
            if (File is not SongItem song) return;

            await _crossProcess.LaunchFile(song);
        }

        private void HandleNudge(MidiEvent midiEvent) 
        {
            var offset = midiEvent.Mapping.Amount;

            if (midiEvent.MidiEventType is MidiEventType.NoteOn)
            {
                _nudgeOffset = offset;

                var newSpeed = SelectedSpeedCurve == MusicSpeedCurveTypes.Logarithmic
                    ? ClampSpeed(_rawSpeedValue.Value.GetNearestPercentValue(offset))
                    : ClampSpeed(_rawSpeedValue.Value + offset);

                SetSpeed(newSpeed);
                return;
            }
            if (midiEvent.MidiEventType is MidiEventType.NoteOff)
            {
                SetSpeed(_rawSpeedValue.Value - _nudgeOffset);
                _nudgeOffset = 0;
            }
        }

        private async Task HandleRestart()
        {
            if (_timer is null) return;

            await CancelSpeed(true);

            if (PlayButtonEnabled)
            {
                await _togglePlay();
            }
            await _restartSubtune(CurrentSubtuneIndex);
            _timer.ResetTimer();
        }

        private async Task StartSong(SongItem s)
        {
            TimedPlayButtonEnabled = false;
            TimedPlayComboBoxEnabled = false;
            ProgressEnabled = true;           

            if (s.Custom is not null && s.Custom.DefaultSpeedCurve != 0) 
            {
                await Task.Delay(300);
                SelectedSpeedCurve = s.Custom.DefaultSpeedCurve;
                _forceSpeedChange = true;
                SetSpeed(s.Custom.DefaultSpeed);
            }
            else if(_rawSpeedValue.Value != 0)
            {
                await Task.Delay(300);
                SelectedSpeedCurve = MusicSpeedCurveTypes.Linear;
                SetSpeed(0);
            }
            _previousRawSpeed = _rawSpeedValue.Value;
            InitializeProgress(s);
        }


        private async Task HandleSeek(TimeSpan targetTime)
        {
            if (_currentSong is null || Progress is null || _timer is null) return;

            if (targetTime == _currentSeekTargetTime) return;

            await CancelSpeed(true);

            TrackSeekInProgress = true;

            try
            {   
                _currentSeekTargetTime = targetTime;

                var togglePlay = PlayButtonEnabled;

                if (togglePlay) 
                {
                    _timer?.ResumeTimer();                    
                }
                PlayButtonEnabled = true;
                PauseButtonEnabled = false;
                var nearlyEndOfSong = targetTime >= Progress.TotalTimeSpan - TimeSpan.FromMilliseconds(500);  //The subtraction avoids a potential loop.

                var seekDirection = targetTime < Progress!.CurrentSpan || nearlyEndOfSong
                    ? SeekDirection.Backward
                    : SeekDirection.Forward;

                if (seekDirection is SeekDirection.Backward)
                {
                    _timer?.ResetTimer();
                }

                var seekSpeed = SelectedSeekSpeed is SeekSpeed.Accurate
                    ? MusicConstants.Log_Speed_Max_Accurate
                    : MusicConstants.Log_Speed_Max;

                await _startSeek(CurrentSubtuneIndex, togglePlay, _muteRandomSeek, seekSpeed, seekDirection);

                _timer?.UpdateSpeed(seekSpeed.GetLogPercentage());
                _midiTrackSeekInProgress = false;

                _fastForwardTimerSubscription?.Dispose();

                _fastForwardTimerSubscription = _timer?.CurrentTime.Subscribe(async currentTime =>
                {
                    await OnSeekTick(currentTime);
                });
            }
            catch (TeensyDjException)
            {   
                await CancelSeek();
                return;
            }
        }

        private async Task OnSeekTick(TimeSpan currentTime)
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
        }

        private async Task<bool> CancelSpeed(bool resumePreviousSpeed = false) 
        {            
            var cancelled = CancelFastForward(resumePreviousSpeed);

            cancelled =  cancelled || await CancelSeek();

            if (cancelled) 
            {
                await Task.Delay(100);
            }
            return cancelled;
        }

        private async Task<bool> CancelSeek()
        {
            if (TrackSeekInProgress is false) return false;

            try
            {
                _currentSeekTargetTime = null;
                _fastForwardTimerSubscription?.Dispose();

                _originalSpeed = SelectedSpeedCurve == MusicSpeedCurveTypes.Linear
                    ? Math.Round(_rawSpeedValue.Value, 2)
                    : _rawSpeedValue.Value.GetLogPercentage();

                _timer?.UpdateSpeed(_originalSpeed);
               
                TrackSeekInProgress = false;
                PlayButtonEnabled = false;
                PauseButtonEnabled = true;

                await _endSeek(_muteRandomSeek, _originalSpeed, SelectedSpeedCurve);
            }
            catch (Exception)
            {
                //swallow the exception.  Not much we can do.  This could be a problemmatic SID.
                _alert.Publish("This SID might be incompatible with DJ functions");
            }
            return true;
        }

        private bool CancelFastForward(bool resumePreviousSpeed = false)
        {
            if (FastForwardInProgress is false) return false;

            PlayButtonEnabled = false;
            PauseButtonEnabled = true;
            FastForwardInProgress = false;
            SetSpeedEnabled = true;
            FastForwardSpeed = FastForwardSpeed.Off;
            SelectedSpeedCurve = _previousSpeedCurve;
            var speed = resumePreviousSpeed ? _previousRawSpeed : 0;
            _shouldEndFastForward = true;
            SetSpeed(speed);

            return true;
        }

        private void HandleHomeSpeed()
        {
            if (_previousRawSpeed == _rawSpeedValue.Value)
            {
                SetSpeed(0);
                return;
            }
            SetSpeed(_previousRawSpeed);
        }

        private void HandleDecrease50()
        {
            if (SelectedSpeedCurve == MusicSpeedCurveTypes.Logarithmic)
            {
                if (_rawSpeedValue.Value == _previousRawSpeed.GetNearestPercentValue(-50) && _rawSpeedValue.Value != 0) return;

                if (_rawSpeedValue.Value == _previousRawSpeed.GetNearestPercentValue(50))
                {
                    SetSpeed(_previousRawSpeed);
                    return;
                }
                SetSpeed(ClampSpeed(_rawSpeedValue.Value.GetNearestPercentValue(-50)));
                return;
            }
            if (_rawSpeedValue.Value == _previousRawSpeed - 50 && _rawSpeedValue.Value != 0) return;

            if (_rawSpeedValue.Value == _previousRawSpeed + 50)
            {
                SetSpeed(_previousRawSpeed);
                return;
            }
            _previousRawSpeed = _rawSpeedValue.Value;

            SetSpeed(ClampSpeed(_rawSpeedValue.Value - 50));
        }

        private void HandleIncrease50()
        {
            if (SelectedSpeedCurve == MusicSpeedCurveTypes.Logarithmic)
            {
                if (_rawSpeedValue.Value == _previousRawSpeed.GetNearestPercentValue(50) && _rawSpeedValue.Value != 0) return;

                if (_rawSpeedValue.Value == _previousRawSpeed.GetNearestPercentValue(-50))
                {
                    SetSpeed(_previousRawSpeed);
                    return;
                }
                SetSpeed(ClampSpeed(_rawSpeedValue.Value.GetNearestPercentValue(50)));
                return;
            }
            if (_rawSpeedValue.Value == _previousRawSpeed + 50 && _rawSpeedValue.Value != 0) return;

            if (_rawSpeedValue.Value == _previousRawSpeed - 50)
            {
                SetSpeed(_previousRawSpeed);
                return;
            }
            _previousRawSpeed = _rawSpeedValue.Value;

            SetSpeed(ClampSpeed(_rawSpeedValue.Value + 50));
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
            if (PlayButtonEnabled) _timer?.ResumeTimer();
            if (PauseButtonEnabled) _timer?.PauseTimer();

            if (await CancelSpeed(true)) 
            {
                return;
            }
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
                    _previousRawSpeed = _rawSpeedValue.Value;
                    _previousSpeedCurve = SelectedSpeedCurve;
                    SelectedSpeedCurve = MusicSpeedCurveTypes.Logarithmic;

                    SetSpeed(33.334);                   
                    return;

                case FastForwardSpeed.Medium:
                    FastForwardSpeed = FastForwardSpeed.MediumFast;
                    SetSpeed(50);
                    return;

                case FastForwardSpeed.MediumFast:
                    FastForwardSpeed = FastForwardSpeed.Fast;
                    SetSpeed(80);
                    return;

                case FastForwardSpeed.Fast:
                    FastForwardSpeed = FastForwardSpeed.Hyper;
                    SetSpeed(MusicConstants.Log_Speed_Max_Accurate);
                    return;

                case FastForwardSpeed.Hyper:
                    CancelFastForward(true);
                    return;
            }
        }

        private void ResetSubtuneButtonState()
        {
            var song = File as SongItem;

            if (song is null) return;
            
            SubtuneNextButtonEnabled = CurrentSubtuneIndex < song.SubtuneLengths.Count;
            SubtunePreviousButtonEnabled = CurrentSubtuneIndex > 1;
        }

        private void InitializeSubtuneProgress(int subtuneIndex) 
        {
            var song = File as SongItem;

            if (song == null || song.SubtuneLengths.Count == 0) return;

            var zeroBasedIndex = subtuneIndex > 0 ? subtuneIndex - 1 : 0;

            var subtuneLength = song.SubtuneLengths[zeroBasedIndex];

            StartTimerObservables(subtuneLength, _playNext);
        }

        private void InitializeProgress(ILaunchableItem? item)
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
            StartTimerObservables(playLength, _playNext);            
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
            await CancelSpeed(true);

            if (CurrentSubtuneIndex > 1)
            {
                InitializeSubtuneProgress(CurrentSubtuneIndex);
                return;
            }
            else 
            {
                InitializeProgress(_currentSong);
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

        public void SetSpeed(double speed) 
        {
            _rawSpeedValue.OnNext(speed);
            RawSpeedValue = speed;            
        }
    }
}
