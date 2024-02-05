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
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Services;

namespace TeensyRom.Ui.Features.Games.State.NewState
{
    public class DirectoryPlayState : PlayerState
    {
        public DirectoryPlayState(FilePlayer playerContext, IMediator mediator, ICachedStorageService storage, ISettingsService settingsService, ILaunchHistory launchHistory, ISnackbarService alert, ISerialStateContext serialContext, INavigationService nav) : base(playerContext, mediator, storage, settingsService, launchHistory, alert, serialContext, nav) { }

        public override bool CanTransitionTo(Type nextStateType)
        {
            return nextStateType == typeof(ShufflePlayState)
                || nextStateType == typeof(SearchPlayState);
        }

        public override void Handle() 
        {
            _directoryState.OnNext(_directoryState.Value);

            if(_selectedGame.Value is not null) SetSelectedGame(_selectedGame.Value);
        }

        public override async Task LoadDirectory(string path, string? filePathToSelect = null)
        {
            var cacheItem = await _storage.GetDirectory(path);

            if (cacheItem == null) return;

            _directoryState.Value.LoadDirectory(cacheItem, filePathToSelect);
            _directoryState.OnNext(_directoryState.Value);
        }

        public override async Task PlayGame(GameItem game)
        {
            var result = await _mediator.Send(new LaunchFileCommand { Path = game.Path });

            if (result.LaunchResult is LaunchFileResultType.ProgramError)
            {
                _alert.Enqueue($"{game.Name} is currently unsupported (see logs).  Skipping to the next game.");
                _storage.MarkIncompatible(game);
                await PlayNext();
                return;
            }

            await base.PlayGame(game);
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
