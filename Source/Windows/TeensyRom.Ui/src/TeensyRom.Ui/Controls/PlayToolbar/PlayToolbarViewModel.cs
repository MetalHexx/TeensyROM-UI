using MediatR;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using TeensyRom.Core.Commands.File.LaunchFile;
using TeensyRom.Core.Common;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Features.Common.Models;
using TeensyRom.Ui.Features.Common.State;
using TeensyRom.Ui.Features.Common.State.Progress;
using TeensyRom.Ui.Features.Music.State;

namespace TeensyRom.Ui.Controls.PlayToolbar
{
    public class PlayToolbarViewModel : ReactiveObject
    {   
        [Reactive] public bool EnablePlayButton { get; set; }
        [Reactive] public bool EnablePauseButton { get; set; }
        [Reactive] public bool EnableStopButton { get; set; }
        [ObservableAsProperty] public bool ShowTitleOnly { get; }
        [ObservableAsProperty] public bool ProgressEnabled { get; }
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
        public ReactiveCommand<Unit, Unit> ToggleShuffleCommand { get; set; }
        public ReactiveCommand<Unit, Unit> FavoriteCommand { get; set; }
        public ReactiveCommand<Unit, Unit> ShareCommand { get; set; }
        public ReactiveCommand<Unit, Unit> NavigateToFileDirCommand { get; set; }

        private readonly IAlertService _alert;
        private IProgressTimer? _timer;
        private IDisposable? _timerCompleteSubscription;
        private IDisposable? _timerTickSubscription;

        public PlayToolbarViewModel(
            IObservable<ILaunchableItem> file, 
            IObservable<LaunchItemState> playState,
            IProgressTimer? timer,
            Func<Unit> toggleMode, 
            Func<Task> togglePlay,
            Func<Task> playPrevious, 
            Func<Task> playNext, 
            Func<ILaunchableItem, Task> saveFav,
            Func<string, Task> loadDirectory,
            IAlertService alert)
        {
            _timer = timer;
            _alert = alert;

            file
                .Where(item => item is not null)
                .ToPropertyEx(this, s => s.File);

            var showReleaseInfo = file
                .Where(item => item is not null)
                .Select(item => !string.IsNullOrWhiteSpace(item.ReleaseInfo));

            showReleaseInfo.ToPropertyEx(this, vm => vm.ShowReleaseInfo);

            var showCreatorInfo = file
                .Where(item => item is not null)
                .Select(item => !string.IsNullOrWhiteSpace(item.Creator));

            showCreatorInfo.ToPropertyEx(this, vm => vm.ShowCreator);

            showReleaseInfo.CombineLatest(showCreatorInfo, (release, creator) => release && creator)
                .ToPropertyEx(this, vm => vm.ShowReleaseCreatorSeperator);

            showReleaseInfo.CombineLatest(showCreatorInfo, (release, creator) => !(release && creator))
                .Select(x => x)
                .ToPropertyEx(this, vm => vm.ShowTitleOnly);

            file
                .Where(s => s is not null)
                .Select(s => !string.IsNullOrEmpty(s.ShareUrl))
                .ToPropertyEx(this, vm => vm.ShareVisible);

            playState
                .Select(state => state.PlayMode == PlayMode.Shuffle)
                .ToPropertyEx(this, vm => vm.ShuffleModeEnabled);

            playState
                .Where(state => state.PlayState != PlayState.Playing)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ =>
                {
                    EnablePauseButton = false;
                    EnableStopButton = false;
                    EnablePlayButton = true;
                    _timer?.PauseTimer();
                });

            var playToggle = playState
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(state => state.PlayState == PlayState.Playing)                
                .Do(_ => EnablePlayButton = false);

            playToggle
                .Where(_ => File is GameItem)
                .Subscribe(_ => 
                {
                    EnableStopButton = true;
                    EnablePauseButton = false;
                });

            playToggle
                .Where(_ => File is SongItem)
                .Subscribe(_ =>
                {
                    EnableStopButton = false;
                    EnablePauseButton = true;
                });

            file
                .OfType<SongItem>()
                .Subscribe(song => InitializeProgress(playNext, song));

            file.Select(file => file is SongItem)
                .ToPropertyEx(this, vm => vm.ProgressEnabled);

            TogglePlayCommand = ReactiveCommand.CreateFromTask(_ => 
            {
                if (EnablePlayButton) _timer?.ResumeTimer();
                if (EnablePauseButton) _timer?.PauseTimer();
                return togglePlay();
            });
            NextCommand = ReactiveCommand.CreateFromTask(playNext);
            PreviousCommand = ReactiveCommand.CreateFromTask(playPrevious);
            ToggleShuffleCommand = ReactiveCommand.Create(toggleMode);
            FavoriteCommand = ReactiveCommand.CreateFromTask(_ => saveFav(File!));
            ShareCommand = ReactiveCommand.Create<Unit, Unit>(_ => HandleShareCommand());
            NavigateToFileDirCommand = ReactiveCommand.CreateFromTask(_ => loadDirectory(File!.Path.GetUnixParentPath()!));
        }

        private void InitializeProgress(Func<Task> playNext, SongItem song)
        {
            if(_timer == null) return;

            _timerTickSubscription?.Dispose();
            _timerCompleteSubscription?.Dispose();

            _timer?.StartNewTimer(song.SongLength);

            _timerCompleteSubscription = _timer?.CurrentTime
                .Select(t => new TimeProgressViewModel(song.SongLength, t))
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, vm => vm.Progress);

            _timerCompleteSubscription = _timer?.TimerComplete
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ =>
                {
                    playNext();
                });
        }

        private Unit HandleShareCommand() 
        {   
            Clipboard.SetText(File!.ShareUrl);
            _alert.Publish("Share URL copied to clipboard.");
            return Unit.Default;
        }
    }
}
