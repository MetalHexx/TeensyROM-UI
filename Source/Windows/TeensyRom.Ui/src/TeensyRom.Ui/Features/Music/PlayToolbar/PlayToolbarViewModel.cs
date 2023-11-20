using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive;
using System.Reactive.Linq;
using TeensyRom.Core.Commands.File.LaunchFile;

namespace TeensyRom.Ui.Features.Music.PlayToolbar
{

    public class PlayToolbarViewModel: ReactiveObject
    {
        [ObservableAsProperty] public Song Song { get; }
        [ObservableAsProperty] public TimeSpan CurrentTime { get; }
        [ObservableAsProperty] public bool ShuffleModeEnabled { get; }

        public  ReactiveCommand<Unit, Unit>  PlayCommand { get; set; }
        
        private readonly ILaunchFileCommand _launchFileCommand;
        private readonly IMusicState _musicState;

        public PlayToolbarViewModel(ILaunchFileCommand launchFileCommand, IMusicState musicState)
        {
            PlayCommand = ReactiveCommand.Create<Unit, Unit>(_ => HandlePlayCommand());
            _launchFileCommand = launchFileCommand;
            _musicState = musicState;

            _musicState.CurrentSong
                .ToPropertyEx(this, s => s.Song);

            _musicState.CurrentSongTime
                .ToPropertyEx(this, vm => vm.CurrentTime);

            _musicState.CurrentSongMode
                .Select(mode => mode == SongMode.Shuffle)
                .ToPropertyEx(this, vm => vm.ShuffleModeEnabled);
        }

        private Unit HandlePlayCommand()
        {
            _musicState.LoadSong(new()
            {
                Path = "/sync/sid/Aces_High.sid",
                ArtistName = "Iron Maiden",
                SongLength = TimeSpan.FromMinutes(1),
                SongName = "Aces_High.sid"

            });
            return Unit.Default;
        }
    }
}
