using DynamicData;
using MaterialDesignThemes.Wpf;
using MediatR;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Commands.File.LaunchFile;
using TeensyRom.Core.Common;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Core.Storage.Services;
using TeensyRom.Ui.Controls.DirectoryTree;
using TeensyRom.Ui.Features.Common.State;
using TeensyRom.Ui.Features.Music.State;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Services;

namespace TeensyRom.Ui.Features.Games.State
{
    public abstract class PlayerState : IPlayerState
    {
        public IObservable<PlayerDirectoryState> DirectoryState => _directoryState.AsObservable();
        public IObservable<GameItem> LaunchedGame => _runningGame.AsObservable();
        public IObservable<GameItem> SelectedGame => _selectedGame.AsObservable();
        public IObservable<PlayPausedState> PlayState => _gameState.AsObservable();

        protected BehaviorSubject<PlayerDirectoryState> _directoryState;
        protected BehaviorSubject<GameItem> _runningGame = new(null!);
        protected BehaviorSubject<GameItem> _selectedGame = new(null!);
        protected BehaviorSubject<PlayPausedState> _gameState = new(PlayPausedState.Stopped);
        protected readonly ICachedStorageService _storage;
        protected readonly ISettingsService _settingsService;
        protected readonly ILaunchHistory _launchHistory;
        protected readonly ISnackbarService _alert;
        private readonly IDirectoryTreeState _tree;
        protected readonly PlayerContext _playerContext;
        protected readonly IMediator _mediator;
        protected IDisposable _settingsSubscription;
        protected TeensySettings _settings;

        public PlayerState(PlayerContext playerContext, IMediator mediator, ICachedStorageService storage, ISettingsService settingsService, ILaunchHistory launchHistory, ISnackbarService alert, ISerialStateContext serialContext, INavigationService nav, IDirectoryTreeState tree)
        {
            _directoryState = new(new PlayerDirectoryState(tree));
            _storage = storage;
            _settingsService = settingsService;
            _launchHistory = launchHistory;
            _alert = alert;
            _tree = tree;
            _playerContext = playerContext;
            _mediator = mediator;
            _playerContext = playerContext;
        }

        public abstract bool CanTransitionTo(Type nextStateType);
        public abstract void Handle();

        //TODO: Move these toolbar functions to another state
        public virtual Task CacheAll() => _storage.CacheAll();

        public virtual Task ClearSearch() => throw new TeensyStateException(InvalidStateExceptionMessage);

        public virtual async Task DeleteFile(GameItem game) => await _storage.DeleteFile(game, _settings.TargetType);

        public virtual async Task LoadDirectory(string path, string? filePathToSelect = null)
        {
            var cacheItem = await _storage.GetDirectory(path);

            if (cacheItem == null) return;

            _directoryState.Value.LoadDirectory(cacheItem, filePathToSelect);
            _directoryState.OnNext(_directoryState.Value);
        }

        public virtual async Task PlayGame(GameItem game)
        {
            var result = await _mediator.Send(new LaunchFileCommand { Path = game.Path });

            if (result.LaunchResult is LaunchFileResultType.ProgramError)
            {
                _alert.Enqueue($"{game.Name} is currently unsupported (see logs).  Skipping to the next game.");
                _storage.MarkIncompatible(game);
                await PlayNext();
                return;
            }
            Application.Current.Dispatcher.Invoke(() =>
            {
                game.IsSelected = true;

                var shouldUpdateCurrent = _runningGame.Value is not null
                    && game.Path.Equals(_runningGame.Value.Path) == false;

                if (shouldUpdateCurrent)
                {
                    _runningGame.Value!.IsSelected = false;
                }
            });
            _runningGame.OnNext(game);
            _gameState.OnNext(PlayPausedState.Playing);
        }

        public virtual Task StopGame()
        {
            _gameState.OnNext(PlayPausedState.Stopped);
            return _mediator.Send(new ResetCommand());
        }

        public virtual Task PlayNext() => throw new TeensyStateException(InvalidStateExceptionMessage);

        public virtual Task PlayPrevious() => throw new TeensyStateException(InvalidStateExceptionMessage);

        public virtual Task<GameItem?> PlayRandom() => throw new TeensyStateException(InvalidStateExceptionMessage);

        public virtual Task RefreshDirectory(bool bustCache = true) => throw new TeensyStateException(InvalidStateExceptionMessage);

        public virtual void ResetDirectoryTree(string path)
        {
            _directoryState.Value.ResetDirectoryTree(path);
            _directoryState.OnNext(_directoryState.Value);
        }

        public virtual async Task SaveFavorite(GameItem game)
        {
            var favGame = await _storage.SaveFavorite(game);
            var gameParentDir = favGame?.Path.GetUnixParentPath();

            if (gameParentDir is null) return;

            var directoryResult = await _storage.GetDirectory(gameParentDir);

            if (directoryResult is null) return;

            _directoryState.Value.LoadDirectory(directoryResult);
        }

        public virtual Unit SearchGames(string searchText) => throw new TeensyStateException(InvalidStateExceptionMessage);

        public virtual Unit SetSelectedGame(GameItem game)
        {
            game.IsSelected = true;

            var shouldUpdateCurrent = _selectedGame.Value is not null
                && game.Path.Equals(_selectedGame.Value.Path) == false;

            if (shouldUpdateCurrent)
            {
                _selectedGame.Value!.IsSelected = false;
            }
            _selectedGame.OnNext(game);
            return Unit.Default;
        }

        public virtual Unit NextPage()
        {
            _directoryState.Value.GoToNextPage();
            _directoryState.OnNext(_directoryState.Value);
            return Unit.Default;
        }
        public virtual Unit PreviousPage()
        {
            _directoryState.Value.GoToPreviousPage();
            _directoryState.OnNext(_directoryState.Value);
            return Unit.Default;
        }
        public virtual Unit SetPageSize(int pageSize)
        {
            _directoryState.Value.SetPageSize(pageSize);
            _directoryState.OnNext(_directoryState.Value);
            return Unit.Default;
        }
        protected string InvalidStateExceptionMessage => $"Cannot perform this operation from: {GetType().Name}";
    }
}
