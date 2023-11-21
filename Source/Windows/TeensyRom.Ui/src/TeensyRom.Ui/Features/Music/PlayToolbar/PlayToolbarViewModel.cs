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

        public  ReactiveCommand<Unit, Unit>  PlayCommand { get; set; }
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

            PlayCommand = ReactiveCommand.Create<Unit, Unit>(_ => HandlePlayCommand());
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

        private Unit HandlePlayCommand()
        {
            _musicState.LoadSong(new()
            {
                Path = "/sync/sid/Aces_High.sid",
                ArtistName = "Iron Maiden",
                SongLength = TimeSpan.FromMinutes(1),
                Name = "Aces_High.sid"

            });
            return Unit.Default;
        }
    }
}
