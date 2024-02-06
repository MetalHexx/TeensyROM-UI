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
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Services;

namespace TeensyRom.Ui.Features.Games.State.NewState
{
    public class NormalPlayState : PlayerState
    {
        public NormalPlayState(PlayerContext playerContext, IMediator mediator, ICachedStorageService storage, ISettingsService settingsService, ILaunchHistory launchHistory, ISnackbarService alert, ISerialStateContext serialContext, INavigationService nav, IDirectoryTreeState tree) : base(playerContext, mediator, storage, settingsService, launchHistory, alert, serialContext, nav, tree) { }

        public override bool CanTransitionTo(Type nextStateType)
        {
            return nextStateType == typeof(ShuffleState)
                || nextStateType == typeof(SearchState);
        }

        public override void Handle() 
        {
            _directoryState.OnNext(_directoryState.Value);

            if(_selectedGame.Value is not null) SetSelectedGame(_selectedGame.Value);
        }

        public override async Task PlayNext()
        {
            var parentPath = _runningGame.Value.Path.GetUnixParentPath();
            var directoryResult = await _storage.GetDirectory(parentPath);

            if (directoryResult is null) return;

            var currentIndex = directoryResult.Files.IndexOf(_runningGame.Value);

            var nextFile = directoryResult.Files.Count == currentIndex + 1
                ? directoryResult.Files.First()
                : directoryResult.Files[++currentIndex];

            if (nextFile is GameItem game)
            {
                if (game.Path == _runningGame.Value.Path) return;

                _selectedGame.OnNext(game);
                await PlayGame(game);
                return;
            }
            await PlayGame(_runningGame.Value);
        }

        public override async Task PlayPrevious()
        {
            var parentPath = _runningGame.Value.Path.GetUnixParentPath();
            var directoryResult = await _storage.GetDirectory(parentPath);

            if (directoryResult is null)
            {
                await PlayGame(_runningGame.Value);
                _selectedGame.OnNext(_runningGame.Value);
                return;
            }
            var gameIndex = directoryResult.Files.IndexOf(_runningGame.Value);

            var game = gameIndex == 0
                ? directoryResult.Files.Last() as GameItem
                : directoryResult.Files[--gameIndex] as GameItem;

            await PlayGame(game!);
            _selectedGame.OnNext(game!);
        }

        public override Task RefreshDirectory(bool bustCache = true)
        {
            if (string.IsNullOrWhiteSpace(_directoryState.Value.CurrentPath)) return Task.CompletedTask;

            if (bustCache) _storage.ClearCache(_directoryState.Value.CurrentPath);

            return LoadDirectory(_directoryState.Value.CurrentPath);
        }
    }
}
