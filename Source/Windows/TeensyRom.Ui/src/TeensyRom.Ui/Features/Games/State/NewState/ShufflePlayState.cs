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

namespace TeensyRom.Ui.Features.Games.State.NewState
{
    public class ShufflePlayState : PlayerState
    {
        public ShufflePlayState(FilePlayer playerContext, IMediator mediator, ICachedStorageService storage, ISettingsService settingsService, ILaunchHistory launchHistory, ISnackbarService alert, ISerialStateContext serialContext, INavigationService nav, IDirectoryTreeState tree) : base(playerContext, mediator, storage, settingsService, launchHistory, alert, serialContext, nav, tree) { }

        public override bool CanTransitionTo(Type nextStateType)
        {
            return nextStateType == typeof(DirectoryPlayState)
                || nextStateType == typeof(SearchPlayState);
        }

        public override void Handle() 
        {
            _directoryState.OnNext(_directoryState.Value);

            if (_selectedGame.Value is not null) SetSelectedGame(_selectedGame.Value);
        }

        public override async Task PlayNext()
        {
            var game = _launchHistory.GetNext(TeensyFileType.Crt, TeensyFileType.Prg) as GameItem;

            if (game is not null)
            {
                await LoadDirectory(game.Path.GetUnixParentPath());
                await PlayGame(game);
                _selectedGame.OnNext(game!);

                return;
            }
            var randomGame = await PlayRandom();
            _selectedGame.OnNext(randomGame!);
        }

        public override async Task PlayPrevious()
        {
            var game = _launchHistory.GetPrevious(TeensyFileType.Prg, TeensyFileType.Crt) as GameItem;

            if (game is not null)
            {
                await LoadDirectory(game.Path.GetUnixParentPath());
                await PlayGame(game);
                _selectedGame.OnNext(game!);
                return;
            }
            await PlayGame(_runningGame.Value);
            _selectedGame.OnNext(_runningGame.Value);
            return;
        }

        public override async Task<GameItem?> PlayRandom()
        {
            var game = _storage.GetRandomFile(TeensyFileType.Crt, TeensyFileType.Prg) as GameItem;

            if (game is not null)
            {
                await LoadDirectory(game.Path.GetUnixParentPath(), game.Path);
                await PlayGame(game);

                _launchHistory.Add(game!);

                return game;
            }
            _alert.Enqueue("Random search requires visiting at least one directory with programs in it first.  Try the cache button next to the dice for best results.");
            return null;
        }

        public override Task RefreshDirectory(bool bustCache = true)
        {
            if (string.IsNullOrWhiteSpace(_directoryState.Value.CurrentPath)) return Task.CompletedTask;

            if (bustCache) _storage.ClearCache(_directoryState.Value.CurrentPath);

            return LoadDirectory(_directoryState.Value.CurrentPath);
        }

        public override async Task SaveFavorite(GameItem game)
        {
            var favGame = await _storage.SaveFavorite(game);
            var gameParentDir = favGame?.Path.GetUnixParentPath();

            if (gameParentDir is null) return;

            var directoryResult = await _storage.GetDirectory(gameParentDir);

            if (directoryResult is null) return;

            _directoryState.Value.LoadDirectory(directoryResult);
        }

        public override Unit ToggleShuffleMode()
        {
            if(_nextMode.Value == NextPreviousMode.Shuffle)
            {
                _nextMode.OnNext(NextPreviousMode.Next);
                return Unit.Default;
            }
            _nextMode.OnNext(NextPreviousMode.Shuffle);
            return Unit.Default;
        }
    }
}
