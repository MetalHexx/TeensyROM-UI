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

namespace TeensyRom.Ui.Features.Games.State.NewState
{
    public class SearchPlayState : PlayerState
    {
        public SearchPlayState(FilePlayer playerContext, IMediator mediator, ICachedStorageService storage, ISettingsService settingsService, ILaunchHistory launchHistory, ISnackbarService alert, ISerialStateContext serialContext, INavigationService nav, IDirectoryTreeState tree) : base(playerContext, mediator, storage, settingsService, launchHistory, alert, serialContext, nav, tree) { }

        public override bool CanTransitionTo(Type nextStateType)
        {
            return nextStateType == typeof(DirectoryPlayState)
                || nextStateType == typeof(ShufflePlayState);
        }

        public override void Handle() => _directoryState.OnNext(_directoryState.Value);

        public override async Task LoadDirectory(string path, string? filePathToSelect = null) => throw new TeensyStateException(InvalidStateExceptionMessage);

        public override Task ClearSearch()
        {
            _directoryState.Value.ClearSelection();
            _directoryState.Value.LoadDirectory([]);
            _directoryState.OnNext(_directoryState.Value);
            return Task.CompletedTask;
        }

        public override async Task PlayNext()
        {
            var currentItem = _directoryState.Value.DirectoryContent
                .FirstOrDefault(i => i.Path.Equals(_runningGame.Value.Path));

            if (currentItem is null) return;

            var currentIndex = _directoryState.Value.DirectoryContent.IndexOf(currentItem);

            var nextGame = _directoryState.Value.DirectoryContent.Count == currentIndex + 1
                ? _directoryState.Value.DirectoryContent.First()
                : _directoryState.Value.DirectoryContent[++currentIndex];

            if (nextGame.Path == currentItem.Path) return;

            if (nextGame is GameItem game)
            {
                await PlayGame(game);
                return;
            }
            await PlayGame(_runningGame.Value);
        }

        public override async Task PlayPrevious()
        {
            var currentItem = _directoryState.Value.DirectoryContent
                .FirstOrDefault(i => i.Path.Equals(_runningGame.Value.Path));

            var currentIndex = _directoryState.Value.DirectoryContent.IndexOf(currentItem);

            var file = _directoryState.Value.DirectoryContent.Count == 0
                ? _directoryState.Value.DirectoryContent.Last()
                : _directoryState.Value.DirectoryContent[--currentIndex];

            if (file is GameItem game)
            {
                await PlayGame(game);
                return;
            }
            await PlayGame(_runningGame.Value);
        }

        public override Unit SearchGames(string searchText)
        {
            var searchResult = _storage.SearchPrograms(searchText)
                .Cast<StorageItem>()
                .Take(100)
                .ToList();

            if (searchResult is null) return Unit.Default;

            _directoryState.Value.LoadDirectory(searchResult, null);

            var firstGame = searchResult.FirstOrDefault();

            if(firstGame is GameItem game)
            {
                _selectedGame.OnNext(game);
            }

            _directoryState.OnNext(_directoryState.Value);

            return Unit.Default;
        }
    }
}
