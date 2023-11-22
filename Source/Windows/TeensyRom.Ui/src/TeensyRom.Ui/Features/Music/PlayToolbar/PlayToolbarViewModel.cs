using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive;
using System.Reactive.Linq;
using TeensyRom.Core.Commands.File.LaunchFile;
using TeensyRom.Ui.Features.Common.Models;

namespace TeensyRom.Ui.Features.Music.PlayToolbar
{

    public class PlayToolbarViewModel: ReactiveObject
    {
        [ObservableAsProperty] public SongItemVm Song { get; }
        [ObservableAsProperty] public TimeSpan CurrentTime { get; }
        [ObservableAsProperty] public bool ShuffleModeEnabled { get; }
        [ObservableAsProperty] public bool IsPlaying { get; }

        public  ReactiveCommand<Unit, Unit>  TogglePlayCommand { get; set; }
        public  ReactiveCommand<Unit, Unit>  PreviousCommand { get; set; }
        public  ReactiveCommand<Unit, Unit>  NextCommand { get; set; }

        private readonly ILaunchFileCommand _launchFileCommand;
        private readonly IMusicState _musicState;

        public PlayToolbarViewModel(ILaunchFileCommand launchFileCommand, IMusicState musicState)
        {
            _launchFileCommand = launchFileCommand;
            _musicState = musicState;

            _musicState.CurrentSong
                .ToPropertyEx(this, s => s.Song);

            _musicState.CurrentSongTime
                .ToPropertyEx(this, vm => vm.CurrentTime);

            _musicState.CurrentSongMode
                .Select(mode => mode == SongMode.Shuffle)
                .ToPropertyEx(this, vm => vm.ShuffleModeEnabled);

            _musicState.PlayState
                .Select(playState => playState == PlayState.Playing)
                .ToPropertyEx(this, vm => vm.IsPlaying);

            TogglePlayCommand = ReactiveCommand.Create<Unit, Unit>(_ => HandleTogglePlayCommand());
            NextCommand = ReactiveCommand.Create<Unit, Unit>(_ => HandleNextCommand());
            PreviousCommand = ReactiveCommand.Create<Unit, Unit>(_ => HandlePreviousCommand());
        }

        private Unit HandlePreviousCommand()
        {
            _musicState.PlayPrevious();
            return Unit.Default;
        }

        private Unit HandleNextCommand()
        {
            _musicState.PlayNext();
            return Unit.Default;
        }

        private Unit HandleTogglePlayCommand()
        {
            _musicState.ToggleMusic();
            return Unit.Default;
        }
    }
}
