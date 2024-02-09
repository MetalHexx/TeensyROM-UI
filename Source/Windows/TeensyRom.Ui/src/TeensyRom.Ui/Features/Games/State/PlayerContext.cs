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
using System.Windows;
using System.Windows.Input;
using TeensyRom.Core.Commands.File.LaunchFile;
using TeensyRom.Core.Common;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Core.Storage.Services;
using TeensyRom.Ui.Controls.DirectoryTree;
using TeensyRom.Ui.Features.Common.State;
using TeensyRom.Ui.Features.Global;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Services;

namespace TeensyRom.Ui.Features.Games.State
{
    public class PlayerContext : IPlayerContext
    {
        public IObservable<PlayerState> CurrentState => _currentState.AsObservable();
        public IObservable<int> CurrentPage => _directoryState.Select(d => d.CurrentPage);
        public IObservable<int> TotalPages => _directoryState.Select(d => d.TotalPages);
        public IObservable<bool> PagingEnabled => _directoryState.Select(d => d.PagingEnabled);
        public IObservable<DirectoryNodeViewModel?> DirectoryTree => _tree.DirectoryTree.AsObservable();
        public IObservable<ObservableCollection<StorageItem>> DirectoryContent => _directoryState.Select(d => d.DirectoryContent);
        public IObservable<GameItem> LaunchedGame => _launchedGame.AsObservable();
        public IObservable<GameItem> SelectedGame => _selectedGame.AsObservable();
        public IObservable<PlayPausedState> PlayState => _playState.AsObservable();
        private string _currentPath = string.Empty;

        private PlayerState? _previousState;
        private readonly BehaviorSubject<PlayerState> _currentState;        
        private readonly BehaviorSubject<GameItem> _launchedGame = new(null!);
        private readonly BehaviorSubject<GameItem> _selectedGame = new(null!);
        private readonly BehaviorSubject<PlayPausedState> _playState = new(PlayPausedState.Stopped);
        protected BehaviorSubject<DirectoryState> _directoryState = new(new());

        private IDisposable? _settingsSubscription;
        private TeensySettings _settings = null!;        
        private readonly IMediator _mediator;
        private readonly ICachedStorageService _storage;
        private readonly ISettingsService _settingsService;
        private readonly ILaunchHistory _launchHistory;
        private readonly ISnackbarService _alert;
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
            _alert = alert;
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
                    state.Value.SelectedGame.Subscribe(_selectedGame.OnNext),
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
                .Do(state => _tree.ResetDirectoryTree(_settings!.GetLibraryPath(TeensyLibraryType.Programs)))
                .Subscribe(async path => await LoadDirectory(path));
        }

        public bool TryTransitionTo(Type nextStateType)
        {
            if (nextStateType == _currentState.Value.GetType()) return true;

            if (_currentState.Value.CanTransitionTo(nextStateType))
            {
                _previousState = _currentState.Value;
                _currentState.OnNext(_states[nextStateType]);
                return true;
            }
            return false;
        }
        public async Task LoadDirectory(string path, string? filePathToSelect = null)
        {
            if (_currentState.Value is SearchState) 
            {
                if(_previousState is not null)
                {
                    TryTransitionTo(_previousState.GetType());
                }
                else
                {
                    TryTransitionTo(typeof(NormalPlayState));
                }                
            }

            var cacheItem = await _storage.GetDirectory(path);

            if (cacheItem == null) return;

            _directoryState.Value.ClearSelection();
            _directoryState.Value.LoadNewDirectory(cacheItem.ToList(), cacheItem.Path, filePathToSelect);
            var firstItem = _directoryState.Value.SelectFirst();

            if (firstItem is not null) _selectedGame.OnNext(firstItem);
            
            _currentPath = cacheItem.Path;            

            Application.Current.Dispatcher.Invoke(() =>
            {
                _tree.Insert(cacheItem.Directories);
                _tree.SelectDirectory(cacheItem.Path);
            });

            _directoryState.OnNext(_directoryState.Value);
        }
        public Task RefreshDirectory(bool bustCache = true)
        {
            var success = TryTransitionTo(typeof(NormalPlayState));

            if (success)
            {
                if (string.IsNullOrWhiteSpace(_currentPath)) return Task.CompletedTask;

                if (bustCache) _storage.ClearCache(_currentPath);

                return LoadDirectory(_currentPath);
            }
            return Task.CompletedTask;
        }

        public virtual async Task PlayGame(GameItem game)
        {
            var result = await _mediator.Send(new LaunchFileCommand { Path = game.Path });

            if (result.LaunchResult is LaunchFileResultType.ProgramError)
            {
                _alert.Enqueue($"{game.Name} is currently unsupported (see logs).  Skipping to the next game.");
                _storage.MarkIncompatible(game);
                await _currentState.Value.GetNext(_launchedGame.Value, _directoryState.Value);
                return;
            }
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                game.IsSelected = true;

                var shouldUpdateCurrent = _launchedGame.Value is not null
                    && game.Path.Equals(_launchedGame.Value.Path) == false;

                if (shouldUpdateCurrent)
                {
                    _launchedGame.Value!.IsSelected = false;
                }
            }));
            _launchedGame.OnNext(game);
            _playState.OnNext(PlayPausedState.Playing);
        }

        public virtual async Task SaveFavorite(GameItem game)
        {
            var favGame = await _storage.SaveFavorite(game);
            var gameParentDir = favGame?.Path.GetUnixParentPath();

            if (gameParentDir is null) return;

            var directoryResult = await _storage.GetDirectory(gameParentDir);

            if (directoryResult is null) return;

            _directoryState.Value.LoadDirectory(directoryResult.ToList(), directoryResult.Path);
        }

        public Task DeleteFile(GameItem file) => _currentState.Value.DeleteFile(file);
        public async Task PlayNext()
        {
            var game = await _currentState.Value.GetNext(_launchedGame.Value, _directoryState.Value);

            if(game is not null)
            {
                await PlayGame(game);
            }
        }
        public async Task PlayPrevious()
        {
            var game = await _currentState.Value.GetPrevious(_launchedGame.Value, _directoryState.Value);

            if (game is not null)
            {
                await PlayGame(game);
            }
        }
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
        public Unit SearchGames(string searchText)
        {
            var success = TryTransitionTo(typeof(SearchState));

            if (success)
            {
                var searchResult = _storage.SearchPrograms(searchText)
                .Cast<StorageItem>()
                .Take(100)
                .ToList();

                if (searchResult is null) return Unit.Default;
                
                _directoryState.Value.ClearSelection();
                _directoryState.Value.LoadDirectory(searchResult, "Search Results:");
                var firstItem = _directoryState.Value.SelectFirst();
               
                if(firstItem is not null)
                {
                    _selectedGame.OnNext(firstItem);
                }

                _directoryState.OnNext(_directoryState.Value);
            }
            return Unit.Default;
        }

        public async Task ClearSearch()
        {
            _directoryState.Value.ClearSelection();
            await LoadDirectory(_currentPath);
        }
        public Unit ToggleShuffleMode()
        {
            if (_currentState.Value is ShuffleState)
            {
                TryTransitionTo(typeof(NormalPlayState));
                return Unit.Default;
            }
            TryTransitionTo(typeof(ShuffleState));

            return Unit.Default;
        }
        public Task CacheAll() => _storage.CacheAll();
        public Unit SetSelectedGame(GameItem game)
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
    }
}