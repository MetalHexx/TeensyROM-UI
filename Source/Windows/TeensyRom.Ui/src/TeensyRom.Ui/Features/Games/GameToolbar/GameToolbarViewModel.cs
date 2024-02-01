using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Storage.Entities;
using System.Reactive.Linq;
using TeensyRom.Core.Common;
using TeensyRom.Ui.Features.Games.State;

namespace TeensyRom.Ui.Features.Games.GameToolbar
{
    public class GameToolbarViewModel : ReactiveObject
    {
        [ObservableAsProperty] public  GameItem Game { get; }
        [ObservableAsProperty] public bool ShuffleModeEnabled { get; }
        [ObservableAsProperty] public bool IsPlaying { get; }

        public ReactiveCommand<Unit, Unit> TogglePlayCommand { get; set; }
        public ReactiveCommand<Unit, Unit> PreviousCommand { get; set; }
        public ReactiveCommand<Unit, Unit> NextCommand { get; set; }
        public ReactiveCommand<Unit, Unit> ToggleShuffleCommand { get; set; }
        public ReactiveCommand<Unit, bool> FavoriteCommand { get; set; }
        public ReactiveCommand<Unit, Unit> NavigateToGameDirCommand { get; set; }

        private readonly IGameState _gameState;

        public GameToolbarViewModel(IGameState musicState)
        {
            _gameState = musicState;

            _gameState.RunningGame
                .Where(s => s is not null)
                .ToPropertyEx(this, s => s.Game);

            _gameState.CurrentGameMode
                .Select(mode => mode == GameMode.Shuffle)
                .ToPropertyEx(this, vm => vm.ShuffleModeEnabled);

            _gameState.CurrentPlayState
                .Select(playState => playState == GameStateType.Playing)
                .ToPropertyEx(this, vm => vm.IsPlaying);

            TogglePlayCommand = ReactiveCommand.Create<Unit, Unit>(_ => HandleTogglePlayCommand());
            NextCommand = ReactiveCommand.Create<Unit, Unit>(_ => HandleNextCommand());
            PreviousCommand = ReactiveCommand.Create<Unit, Unit>(_ => HandlePreviousCommand());
            ToggleShuffleCommand = ReactiveCommand.Create<Unit, Unit>(_ => musicState.ToggleShuffleMode());
            FavoriteCommand = ReactiveCommand.CreateFromTask(_ => musicState.SaveFavorite(Game!));
            NavigateToGameDirCommand = ReactiveCommand.CreateFromTask(_ => musicState.LoadDirectory(Game!.Path.GetUnixParentPath()!));
        }

        private Unit HandlePreviousCommand()
        {
            _gameState.PlayPrevious();
            return Unit.Default;
        }

        private Unit HandleNextCommand()
        {
            _gameState.PlayNext();
            return Unit.Default;
        }

        private Unit HandleTogglePlayCommand()
        {
            _gameState.ToggleGame();
            return Unit.Default;
        }
    }
}
