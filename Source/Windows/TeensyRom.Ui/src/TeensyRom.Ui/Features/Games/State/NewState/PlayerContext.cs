using MediatR;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows.Input;
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
    public class PlayerContext : IPlayerContext
    {
        public IObservable<PlayerState> CurrentState => _currentState.AsObservable();
        public IObservable<int> CurrentPage => _currentPage.AsObservable();
        public IObservable<int> TotalPages => _totalPages.AsObservable();
        public IObservable<bool> PagingEnabled => _pagingEnabled.AsObservable();
        public IObservable<DirectoryNodeViewModel?> DirectoryTree => _tree.DirectoryTree.AsObservable();
        public IObservable<ObservableCollection<StorageItem>> DirectoryContent => _directoryContent.AsObservable();
        public IObservable<GameItem> RunningGame => _runningGame.AsObservable();
        public IObservable<GameItem> SelectedGame => _selectedGame.AsObservable();
        public IObservable<GameStateType> PlayState => _playState.AsObservable();
        public IObservable<GameItem> GameLaunched => _launchedGame.AsObservable();

        private readonly BehaviorSubject<PlayerState> _currentState;
        private PlayerState _previousState;
        private readonly BehaviorSubject<int> _currentPage = new(1);
        private readonly BehaviorSubject<int> _totalPages = new(1);
        private readonly BehaviorSubject<bool> _pagingEnabled = new(false);
        private readonly BehaviorSubject<ObservableCollection<StorageItem>> _directoryContent = new([]);
        private readonly BehaviorSubject<GameItem> _runningGame = new(null);
        private readonly BehaviorSubject<GameItem> _selectedGame = new(null);
        private readonly BehaviorSubject<GameStateType> _playState = new(GameStateType.Stopped);
        private Subject<GameItem> _launchedGame = new();
        private IDisposable _settingsSubscription;
        private TeensySettings _settings;
        private readonly DirectoryState _directoryState;
        private readonly IMediator _mediator;
        private readonly ICachedStorageService _storage;
        private readonly ISettingsService _settingsService;
        private readonly ILaunchHistory _launchHistory;
        private readonly ISerialStateContext _serialContext;
        private readonly INavigationService _nav;
        private readonly IGameDirectoryTreeState _tree;
        private readonly Dictionary<Type, PlayerState> _states;
        private readonly List<IDisposable> _stateSubscriptions = new();

        public PlayerContext(IMediator mediator, ICachedStorageService storage, ISettingsService settingsService, ILaunchHistory launchHistory, ISnackbarService alert, ISerialStateContext serialContext, INavigationService nav, IGameDirectoryTreeState tree)
        {
            _mediator = mediator;
            _storage = storage;
            _settingsService = settingsService;
            _launchHistory = launchHistory;
            _serialContext = serialContext;
            _nav = nav;
            _tree = tree;
            _states = new()
            {
                { typeof(NormalPlayState), new NormalPlayState(this, mediator, storage, settingsService, launchHistory, alert, serialContext, nav, tree) },
                { typeof(ShuffleState), new ShuffleState(this, mediator, storage, settingsService, launchHistory, alert, serialContext, nav, tree) },
                { typeof(SearchState), new SearchState(this, mediator, storage, settingsService, launchHistory, alert, serialContext, nav, tree) }
            };
            SubscribeToStateObservables();
            _currentState = new(_states[typeof(NormalPlayState)]);

        }

        private void SubscribeToStateObservables()
        {
            foreach (var state in _states)
            {
                _stateSubscriptions.AddRange(
                [
                    state.Value.DirectoryState.Select(s => s.CurrentPage).Subscribe(_currentPage.OnNext),
                    state.Value.DirectoryState.Select(s => s.DirectoryContent).Subscribe(_directoryContent.OnNext),
                    state.Value.DirectoryState.Select(s => s.PagingEnabled).Subscribe(_pagingEnabled.OnNext),
                    state.Value.DirectoryState.Select(s => s.TotalPages).Subscribe(_totalPages.OnNext),
                    state.Value.SelectedGame.Subscribe(_selectedGame.OnNext),
                    state.Value.LaunchedGame.Where(g => g is not null).Subscribe(_runningGame.OnNext),
                    state.Value.LaunchedGame.Subscribe(_launchedGame.OnNext),
                    state.Value.PlayState.Subscribe(_playState.OnNext),
                ]);
            }

            _settingsSubscription = _settingsService.Settings
                .Do(settings => _settings = settings)
                .CombineLatest(_serialContext.CurrentState, _nav.SelectedNavigationView, (settings, serial, navView) => (settings, serial, navView))
                .Where(state => state.serial is SerialConnectedState)
                .Where(state => state.navView?.Type == NavigationLocation.Games)
                .Select(state => (path: state.settings.GetLibraryPath(TeensyLibraryType.Programs), state.settings.TargetType))
                .DistinctUntilChanged()
                .Select(storage => storage.path)
                .Do(state => _currentState.Value.ResetDirectoryTree(_settings!.GetLibraryPath(TeensyLibraryType.Programs)))
                .Subscribe(async path => await _currentState.Value.LoadDirectory(path));
        }

        public bool TryTransitionTo(Type nextStateType)
        {
            if (nextStateType == _currentState.Value.GetType()) return true;

            if (_currentState.Value.CanTransitionTo(nextStateType))
            {
                _previousState = _currentState.Value;
                _currentState.OnNext(_states[nextStateType]);
                _currentState.Value.Handle();
                return true;
            }
            return false;
        }
        public async Task LoadDirectory(string path, string? filePathToSelect = null)
        {
            if (_currentState.Value is SearchState) await ClearSearch();

            await _currentState.Value.LoadDirectory(path, filePathToSelect);            
        }
        public Task RefreshDirectory(bool bustCache = true)
        {
            var success = TryTransitionTo(typeof(NormalPlayState));

            if (success)
            {
                _currentState.Value.RefreshDirectory(bustCache);
            }
            return Task.CompletedTask;
        }
        public Task PlayGame(GameItem game) => _currentState.Value.PlayGame(game);
        public Task SaveFavorite(GameItem game) => _currentState.Value.SaveFavorite(game);
        public Task DeleteFile(GameItem file) => _currentState.Value.DeleteFile(file);
        public Task PlayNext() => _currentState.Value.PlayNext();
        public Task PlayPrevious() => _currentState.Value.PlayPrevious();
        public Task StopGame() => _currentState.Value.StopGame();
        public Task<GameItem?> PlayRandom()
        {
            var success = TryTransitionTo(typeof(ShuffleState));

            if (success)
            {
                return _currentState.Value.PlayRandom();
            }
            return Task.FromResult(null as GameItem);
        }
        public Unit SearchGames(string keyword)
        {
            var success = TryTransitionTo(typeof(SearchState));

            if (success)
            {
                return _currentState.Value.SearchGames(keyword);
            }
            return Unit.Default;
        }
        public async Task ClearSearch()
        {
            if (_currentState.Value is SearchState)
            {
                await _currentState.Value.ClearSearch();
                TryTransitionTo(_previousState.GetType());
            }
        }
        public Unit ToggleShuffleMode()
        {
            if(_currentState.Value is ShuffleState)
            {
                TryTransitionTo(typeof(NormalPlayState));
                return Unit.Default;
            }
            TryTransitionTo(typeof(ShuffleState));

            return Unit.Default;
        }
        public Task CacheAll() => _currentState.Value.CacheAll();
        public Unit SetSelectedGame(GameItem game) => _currentState.Value.SetSelectedGame(game);
        public Unit NextPage() => _currentState.Value.NextPage();
        public Unit PreviousPage() => _currentState.Value.PreviousPage();
        public Unit SetPageSize(int pageSize) => _currentState.Value.SetPageSize(pageSize);
    }
}