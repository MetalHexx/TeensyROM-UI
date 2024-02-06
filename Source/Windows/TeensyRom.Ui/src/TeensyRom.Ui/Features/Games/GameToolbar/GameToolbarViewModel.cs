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
using TeensyRom.Ui.Features.Files.State;
using TeensyRom.Ui.Features.Games.State.NewState;

namespace TeensyRom.Ui.Features.Games.GameToolbar
{
    public class GameToolbarViewModel : ReactiveObject
    {
        [ObservableAsProperty] public  GameItem Game { get; }
        [ObservableAsProperty] public bool ShuffleModeEnabled { get; }
        [ObservableAsProperty] public bool EnableShuffleModeButton { get; }
        [ObservableAsProperty] public bool IsPlaying { get; }

        public ReactiveCommand<Unit, Unit> PlayCommand { get; set; }
        public ReactiveCommand<Unit, Unit> StopCommand { get; set; }
        public ReactiveCommand<Unit, Unit> PreviousCommand { get; set; }
        public ReactiveCommand<Unit, Unit> NextCommand { get; set; }
        public ReactiveCommand<Unit, Unit> ToggleShuffleCommand { get; set; }
        public ReactiveCommand<Unit, Unit> FavoriteCommand { get; set; }
        public ReactiveCommand<Unit, Unit> NavigateToGameDirCommand { get; set; }

        private readonly IPlayerContext _gameState;

        public GameToolbarViewModel(IPlayerContext gameState)
        {
            _gameState = gameState;

            _gameState.RunningGame.ToPropertyEx(this, s => s.Game);

            _gameState.CurrentState
                .Select(state => state is ShuffleState)
                .ToPropertyEx(this, vm => vm.ShuffleModeEnabled);

            _gameState.PlayState
                .Select(playState => playState == PlayPausedState.Playing)
                .ToPropertyEx(this, vm => vm.IsPlaying);

            _gameState.CurrentState
                .Select(state => state is not SearchState)
                .ToPropertyEx(this, vm => vm.EnableShuffleModeButton);

            PlayCommand = ReactiveCommand.CreateFromTask(
                execute: async () => await gameState.PlayGame(Game!),
                outputScheduler: RxApp.MainThreadScheduler);

            StopCommand = ReactiveCommand.CreateFromTask(
                execute: gameState.StopGame,
                outputScheduler: RxApp.MainThreadScheduler);

            NextCommand = ReactiveCommand.CreateFromTask(
                execute: gameState.PlayNext,
                outputScheduler: RxApp.MainThreadScheduler);

            PreviousCommand = ReactiveCommand.CreateFromTask(
                execute: gameState.PlayPrevious,
                outputScheduler: RxApp.MainThreadScheduler);

            ToggleShuffleCommand = ReactiveCommand.Create<Unit, Unit>(
                execute: _ => gameState.ToggleShuffleMode(),
                outputScheduler: RxApp.MainThreadScheduler);

            FavoriteCommand = ReactiveCommand.CreateFromTask(
                execute: _ => gameState.SaveFavorite(Game!), 
                outputScheduler: RxApp.MainThreadScheduler);

            NavigateToGameDirCommand = ReactiveCommand.CreateFromTask(
                execute: _ => gameState.LoadDirectory(Game!.Path.GetUnixParentPath()!),
                outputScheduler: RxApp.MainThreadScheduler);
        }
    }
}