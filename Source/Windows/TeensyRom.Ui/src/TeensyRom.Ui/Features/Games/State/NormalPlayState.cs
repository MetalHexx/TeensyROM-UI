﻿using MediatR;
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
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Services;

namespace TeensyRom.Ui.Features.Games.State
{
    public class NormalPlayState : PlayerState
    {
        public NormalPlayState(PlayerContext playerContext, IMediator mediator, ICachedStorageService storage, ISettingsService settingsService, ILaunchHistory launchHistory, ISnackbarService alert, ISerialStateContext serialContext, INavigationService nav, IDirectoryTreeState tree) : base(playerContext, mediator, storage, settingsService, launchHistory, alert, serialContext, nav, tree) { }

        public override bool CanTransitionTo(Type nextStateType)
        {
            return nextStateType == typeof(ShuffleState)
                || nextStateType == typeof(SearchState);
        }

        public override async Task<GameItem?> GetNext(GameItem currentGame, DirectoryState directoryState)
        {
            var parentPath = currentGame.Path.GetUnixParentPath();
            var directoryResult = await _storage.GetDirectory(parentPath);

            if (directoryResult is null) return null;

            var currentIndex = directoryResult.Files.IndexOf(currentGame);

            var nextFile = directoryResult.Files.Count == currentIndex + 1
                ? directoryResult.Files.First()
                : directoryResult.Files[++currentIndex];

            if (nextFile is GameItem game)
            {
                if (game.Path == currentGame.Path) return null;

                _selectedGame.OnNext(game);
                return game;
            }
            return null;
        }

        public override async Task<GameItem?> GetPrevious(GameItem currentGame, DirectoryState directoryState)
        {
            var parentPath = currentGame.Path.GetUnixParentPath();
            var directoryResult = await _storage.GetDirectory(parentPath);

            if (directoryResult is null)
            {
                _selectedGame.OnNext(currentGame);
                return currentGame;                
            }
            var gameIndex = directoryResult.Files.IndexOf(currentGame);

            var game = gameIndex == 0
                ? directoryResult.Files.Last() as GameItem
                : directoryResult.Files[--gameIndex] as GameItem;
            
            _selectedGame.OnNext(game!);
            return game!;            
        }
    }
}