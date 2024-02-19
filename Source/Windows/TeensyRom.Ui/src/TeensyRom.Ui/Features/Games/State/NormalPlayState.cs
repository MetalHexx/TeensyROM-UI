using MediatR;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
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
    public class NormalPlayState : PlayerState
    {
        public NormalPlayState(PlayerContext playerContext, IMediator mediator, ICachedStorageService storage, ISettingsService settingsService, ILaunchHistory launchHistory, ISnackbarService alert, ISerialStateContext serialContext, INavigationService nav, IDirectoryTreeState tree) : base(playerContext, mediator, storage, settingsService, launchHistory, alert) { }

        public override bool CanTransitionTo(Type nextStateType)
        {
            return nextStateType == typeof(ShuffleState)
                || nextStateType == typeof(SearchState);
        }

        public override async Task<ILaunchableItem?> GetNext(ILaunchableItem currentLaunchable, DirectoryState directoryState)
        {
            var parentPath = currentLaunchable.Path.GetUnixParentPath();
            var directoryResult = await _storage.GetDirectory(parentPath);

            if (directoryResult is null) return null;

            var currentIndex = directoryResult.Files.IndexOf(currentLaunchable);

            var nextFile = directoryResult.Files.Count == currentIndex + 1
                ? directoryResult.Files.First()
                : directoryResult.Files[++currentIndex];

            if (nextFile is GameItem game)
            {
                if (game.Path == currentLaunchable.Path) return null;

                await _playerContext.LoadDirectory(game.Path.GetUnixParentPath(), game.Path);
                return game;
            }
            return null;
        }

        public override async Task<ILaunchableItem?> GetPrevious(ILaunchableItem currentGame, DirectoryState directoryState)
        {
            var parentPath = currentGame.Path.GetUnixParentPath();
            var directoryResult = await _storage.GetDirectory(parentPath);
            var launchableItems = directoryResult?.Files.OfType<ILaunchableItem>().ToList();

            if (launchableItems is null)
            {
                return currentGame;                
            }
            var gameIndex = launchableItems.IndexOf(currentGame);

            var game = gameIndex == 0
                ? launchableItems.Last()
                : launchableItems[--gameIndex];

            if (game is null) return null;

            await _playerContext.LoadDirectory(game.Path.GetUnixParentPath(), game.Path);            
            return game;            
        }
    }
}
