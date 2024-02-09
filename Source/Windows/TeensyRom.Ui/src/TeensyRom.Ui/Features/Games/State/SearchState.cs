using MediatR;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Documents;
using TeensyRom.Core.Commands.File.LaunchFile;
using TeensyRom.Core.Common;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Core.Storage.Services;
using TeensyRom.Ui.Controls.DirectoryTree;
using TeensyRom.Ui.Features.Common.State;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Services;

namespace TeensyRom.Ui.Features.Games.State
{
    public class SearchState : PlayerState
    {
        public SearchState(PlayerContext playerContext, IMediator mediator, ICachedStorageService storage, ISettingsService settingsService, ILaunchHistory launchHistory, ISnackbarService alert, ISerialStateContext serialContext, INavigationService nav, IDirectoryTreeState tree) : base(playerContext, mediator, storage, settingsService, launchHistory, alert) { }

        public override bool CanTransitionTo(Type nextStateType)
        {
            return nextStateType == typeof(NormalPlayState)
                || nextStateType == typeof(ShuffleState);
        }

        public override async Task<GameItem?> GetNext(GameItem currentGame, DirectoryState directoryState)
        {
            await Task.CompletedTask;

            var currentIndex = directoryState.DirectoryContent.IndexOf(currentGame);

            var nextGame = directoryState.DirectoryContent.Count == currentIndex + 1
                ? directoryState.DirectoryContent.First()
                : directoryState.DirectoryContent[++currentIndex];

            if (nextGame.Path == currentGame.Path) return null;

            if (nextGame is GameItem game)
            {
                return game;                
            }
            return currentGame;
        }

        public override async Task<GameItem?> GetPrevious(GameItem currentGame, DirectoryState directoryState)
        {
            await Task.CompletedTask;

            var currentIndex = directoryState.DirectoryContent.IndexOf(currentGame);

            var file = directoryState.DirectoryContent.Count == 0
                ? directoryState.DirectoryContent.Last()
                : directoryState.DirectoryContent[--currentIndex];

            if (file is GameItem game)
            {
                return game;                
            }
            return currentGame;
        }
    }
}
