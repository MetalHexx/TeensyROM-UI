using MediatR;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using TeensyRom.Core.Commands.File.LaunchFile;
using TeensyRom.Core.Common;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Core.Storage.Services;
using TeensyRom.Ui.Controls.DirectoryTree;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Services;

namespace TeensyRom.Ui.Features.Games.State
{
    public class ShuffleState : PlayerState
    {
        public ShuffleState(PlayerContext playerContext, IMediator mediator, ICachedStorageService storage, ISettingsService settingsService, ILaunchHistory launchHistory, ISnackbarService alert, ISerialStateContext serialContext, INavigationService nav, IDirectoryTreeState tree) : base(playerContext, mediator, storage, settingsService, launchHistory, alert, serialContext, nav, tree) { }

        public override bool CanTransitionTo(Type nextStateType)
        {
            return nextStateType == typeof(NormalPlayState)
                || nextStateType == typeof(SearchState);
        }

        public override async Task<GameItem?> GetNext(GameItem currentGame, DirectoryState directoryState)
        {
            var game = _launchHistory.GetNext(TeensyFileType.Crt, TeensyFileType.Prg) as GameItem;

            if (game is not null)
            {
                await _playerContext.LoadDirectory(game.Path.GetUnixParentPath());
                await _playerContext.PlayGame(game);
                _selectedGame.OnNext(game!);
                return game;
            }
            var randomGame = await PlayRandom();
            _selectedGame.OnNext(randomGame!);
            return randomGame;
        }

        public override async Task<GameItem?> GetPrevious(GameItem currentGame, DirectoryState directoryState)
        {
            var game = _launchHistory.GetPrevious(TeensyFileType.Prg, TeensyFileType.Crt) as GameItem;

            if (game is not null)
            {
                await _playerContext.LoadDirectory(game.Path.GetUnixParentPath());
                _selectedGame.OnNext(game!);
                return game;
            }
            _selectedGame.OnNext(currentGame);
            return currentGame;
        }

        public override async Task<GameItem?> PlayRandom()
        {
            var game = _storage.GetRandomFile(TeensyFileType.Crt, TeensyFileType.Prg) as GameItem;

            if (game is not null)
            {
                await _playerContext.LoadDirectory(game.Path.GetUnixParentPath(), game.Path);
                await _playerContext.PlayGame(game);

                _launchHistory.Add(game!);

                return game;
            }
            _alert.Enqueue("Random search requires visiting at least one directory with programs in it first.  Try the cache button next to the dice for best results.");
            return null;
        }
    }
}
