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

namespace TeensyRom.Ui.Controls.PlayToolbar
{
    public class PlayToolbarViewModel : ReactiveObject
    {
        private readonly IAlertService _alert;

        [Reactive] public bool ProgressEnabled {get; set;}        
        [ObservableAsProperty] public ILaunchableItem? File { get; }
        [ObservableAsProperty] public TimeProgressViewModel? Progress { get; } = null;
        [ObservableAsProperty] public string CurrentTime { get; } = string.Empty;
        [ObservableAsProperty] public double CurrentProgress { get; }
        [ObservableAsProperty] public bool ShuffleModeEnabled { get; }
        [ObservableAsProperty] public bool ShareVisible { get; }
        [ObservableAsProperty] public bool IsPlaying { get; }        
        [ObservableAsProperty] public bool IsPaused { get; }        
        [ObservableAsProperty] public bool IsStopped { get; }
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

        public PlayToolbarViewModel(
            IObservable<ILaunchableItem> file, 
            IObservable<LaunchItemState> fileState, 
            IObservable<TimeProgressViewModel>? progress, 
            Func<Unit> toggleMode, 
            Func<Task> togglePlay,
            Func<Task> playPrevious, 
            Func<Task> playNext, 
            Func<ILaunchableItem, Task> saveFav,
            Func<string, Task> loadDirectory,
            IAlertService alert)
        {
            _alert = alert;

            file
                .Where(item => item is not null)
                .ToPropertyEx(this, s => s.File);

            file
                .Where(item => item is not null)
                .Select(item => !string.IsNullOrWhiteSpace(item.ReleaseInfo))
                .ToPropertyEx(this, vm => vm.ShowReleaseInfo);

            file
                .Where(item => item is not null)
                .Select(item => !string.IsNullOrWhiteSpace(item.Creator))
                .ToPropertyEx(this, vm => vm.ShowCreator);

            file
                .Where(item => item is not null)
                .Select(item => !string.IsNullOrWhiteSpace(item.ReleaseInfo) && !string.IsNullOrWhiteSpace(item.Creator))
                .ToPropertyEx(this, vm => vm.ShowReleaseCreatorSeperator);

            if (progress is not null)
            {
                ProgressEnabled = true;

                progress
                    .Where(time => time is not null)
                    .ToPropertyEx(this, vm => vm.Progress);
            }

            file
                .Where(s => s is not null)
                .Select(s => !string.IsNullOrEmpty(s.ShareUrl))
                .ToPropertyEx(this, vm => vm.ShareVisible);

            fileState
                .Select(state => state.PlayMode == PlayMode.Shuffle)
                .ToPropertyEx(this, vm => vm.ShuffleModeEnabled);

            fileState
                .Select(fileState => fileState.PlayState == PlayState.Playing)
                .ToPropertyEx(this, vm => vm.IsPlaying);

            TogglePlayCommand = ReactiveCommand.CreateFromTask(togglePlay);
            NextCommand = ReactiveCommand.CreateFromTask(playNext);
            PreviousCommand = ReactiveCommand.CreateFromTask(playPrevious);
            ToggleShuffleCommand = ReactiveCommand.Create(toggleMode);
            FavoriteCommand = ReactiveCommand.CreateFromTask(_ => saveFav(File!));
            ShareCommand = ReactiveCommand.Create<Unit, Unit>(_ => HandleShareCommand());
            NavigateToFileDirCommand = ReactiveCommand.CreateFromTask(_ => loadDirectory(File!.Path.GetUnixParentPath()!));
        }

        private Unit HandleShareCommand() 
        {   
            Clipboard.SetText(File!.ShareUrl);
            _alert.Publish("Share URL copied to clipboard.");
            return Unit.Default;
        }
    }
}
