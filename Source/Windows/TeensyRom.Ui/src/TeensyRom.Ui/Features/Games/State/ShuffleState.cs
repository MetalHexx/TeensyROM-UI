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
using TeensyRom.Ui.Features.Games.State.Directory;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Services;

namespace TeensyRom.Ui.Features.Games.State
{
    public class ShuffleState : PlayerState
    {
        public ShuffleState(PlayerContext playerContext, IMediator mediator, ICachedStorageService storage, ISettingsService settingsService, ILaunchHistory launchHistory, ISnackbarService alert, ISerialStateContext serialContext, INavigationService nav, IDirectoryTreeState tree) : base(playerContext, mediator, storage, settingsService, launchHistory, alert) { }

        public override bool CanTransitionTo(Type nextStateType)
        {
            return nextStateType == typeof(NormalPlayState)
                || nextStateType == typeof(SearchState);
        }

        public override async Task<FileItem?> GetNext(FileItem currentGame, DirectoryState directoryState)
        {
            var game = _launchHistory.GetNext(TeensyFileType.Crt, TeensyFileType.Prg) as FileItem;

            if (game is not null)
            {
                await _playerContext.LoadDirectory(game.Path.GetUnixParentPath(), game.Path);
                return game;
            }
            var randomGame = _storage.GetRandomFile(TeensyFileType.Prg, TeensyFileType.Crt);
            
            if(randomGame is not null)
            {
                await _playerContext.LoadDirectory(randomGame.Path.GetUnixParentPath(), randomGame.Path);
            }
            return randomGame as FileItem;            
        }

        public override async Task<FileItem?> GetPrevious(FileItem currentGame, DirectoryState directoryState)
        {
            var game = _launchHistory.GetPrevious(TeensyFileType.Prg, TeensyFileType.Crt);

            if (game is not null)
            {
                await _playerContext.LoadDirectory(game.Path.GetUnixParentPath(), game.Path);
                return game;
            }
            return currentGame;
        }
    }
}
