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
using TeensyRom.Core.Common;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Core.Storage.Services;
using TeensyRom.Ui.Features.Common.State;
using TeensyRom.Ui.Features.Music.State;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Services;

namespace TeensyRom.Ui.Features.Games.State.NewState
{
    public abstract class PlayerState : IPlayerState
    {
        public IObservable<PlayerDirectoryState> DirectoryState => _directoryState.AsObservable();
        public IObservable<GameItem> LaunchedGame => _runningGame.AsObservable();
        public IObservable<GameItem> SelectedGame => _selectedGame.AsObservable(); 
        public IObservable<GameStateType> PlayState => _gameState.AsObservable();
        public IObservable<NextPreviousMode> NextMode => _nextMode.AsObservable();

        protected BehaviorSubject<PlayerDirectoryState> _directoryState;
        protected BehaviorSubject<GameItem> _runningGame = new(null!);
        protected BehaviorSubject<GameItem> _selectedGame = new(null!);
        protected BehaviorSubject<GameStateType> _gameState = new(GameStateType.Stopped);
        protected BehaviorSubject<NextPreviousMode> _nextMode = new(State.NextPreviousMode.Next);
        protected readonly ICachedStorageService _storage;
        protected readonly ISettingsService _settingsService;
        protected readonly ILaunchHistory _launchHistory;
        protected readonly ISnackbarService _alert;
        protected readonly FilePlayer _playerContext;
        protected readonly IMediator _mediator;
        protected IDisposable _settingsSubscription;
        protected TeensySettings _settings;

        public PlayerState(FilePlayer playerContext, IMediator mediator, ICachedStorageService storage, ISettingsService settingsService, ILaunchHistory launchHistory, ISnackbarService alert, ISerialStateContext serialContext, INavigationService nav)
        {
            _directoryState = new(new PlayerDirectoryState());
            _storage = storage;
            _settingsService = settingsService;
            _launchHistory = launchHistory;
            _alert = alert;
            _playerContext = playerContext;
            _mediator = mediator;
            _playerContext = playerContext;
        }

        public abstract bool CanTransitionTo(Type nextStateType);
        public abstract void Handle();

        //TODO: Move these toolbar functions to another state
        public virtual Task CacheAll() => _storage.CacheAll();

        public virtual Task ClearSearch() => throw new TeensyStateException(ExceptionMessage);

        public virtual async Task DeleteFile(GameItem game) => await _storage.DeleteFile(game, _settings.TargetType);

        public virtual Task LoadDirectory(string path, string? filePathToSelect = null) => throw new TeensyStateException(ExceptionMessage);

        public virtual Task PlayGame(GameItem game)
        {
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
            _gameState.OnNext(GameStateType.Playing);
            return Task.CompletedTask;
        }

        public virtual Task StopGame() 
        {   
            _gameState.OnNext(GameStateType.Stopped);
            return _mediator.Send(new ResetCommand());
        }

        public virtual Task PlayNext() => throw new TeensyStateException(ExceptionMessage);

        public virtual Task PlayPrevious() => throw new TeensyStateException(ExceptionMessage);

        public virtual Task<GameItem?> PlayRandom() => throw new TeensyStateException(ExceptionMessage);

        public virtual Task RefreshDirectory(bool bustCache = true) => throw new TeensyStateException(ExceptionMessage);

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

        public virtual Unit SearchGames(string searchText) => throw new TeensyStateException(ExceptionMessage);

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

        public virtual Unit ToggleShuffleMode() => throw new TeensyStateException(ExceptionMessage);

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
        private string ExceptionMessage => $"Cannot perform this operation from: {GetType().Name}";
    }
}
