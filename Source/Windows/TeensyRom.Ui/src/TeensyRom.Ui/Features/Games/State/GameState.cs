using MediatR;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Commands.File.LaunchFile;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Core.Storage.Services;
using TeensyRom.Ui.Controls.DirectoryTree;
using TeensyRom.Ui.Features.Global;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Services;
using System.Reactive.Linq;
using System.Windows;
using TeensyRom.Core.Common;
using DynamicData;
using System.Reflection;
using System.IO;
using TeensyRom.Ui.Features.Common.State;
using System.Runtime.CompilerServices;

namespace TeensyRom.Ui.Features.Games.State
{
    public class GameState : IGameState, IDisposable
    {
        public IObservable<int> CurrentPage => _directoryState.CurrentPage;
        public IObservable<int> TotalPages => _directoryState.TotalPages;
        public IObservable<bool> PagingEnabled => _directoryState.PagingEnabled;
        public IObservable<DirectoryNodeViewModel> DirectoryTree => _directoryState.DirectoryTree;
        public IObservable<ObservableCollection<StorageItem>> DirectoryContent => _directoryState.DirectoryContent;
        public IObservable<GameItem> RunningGame => _runningGame.AsObservable();
        public IObservable<GameItem> SelectedGame => _selectedGame.AsObservable();
        public IObservable<NextPreviousMode> CurrentGameMode => _gameMode.AsObservable();
        public IObservable<GameStateType> CurrentPlayState => _playState.AsObservable();
        public IObservable<GameItem> GameLaunched => _gameLaunched.AsObservable();
        public IObservable<bool> SearchEnabled => _searchEnabled.AsObservable();

        private readonly BehaviorSubject<GameItem> _runningGame = new(null);
        private readonly BehaviorSubject<GameItem> _selectedGame = new(null);
        private readonly BehaviorSubject<NextPreviousMode> _gameMode = new(NextPreviousMode.Next);
        private readonly BehaviorSubject<GameStateType> _playState = new(GameStateType.Stopped);
        private readonly BehaviorSubject<bool> _searchEnabled = new(false);
        private List<FileItem> _searchResults = [];

        private Subject<GameItem> _gameLaunched = new();

        private DirectoryState _directoryState;
        private readonly IMediator _mediator;
        private readonly ICachedStorageService _storage;
        private readonly ISettingsService _settingsService;
        private readonly ILaunchHistory _launchHistory;
        private readonly ISnackbarService _alert;
        private TeensySettings _settings = new();

        private IDisposable _settingsSubscription;
        private IDisposable _currentTimeSubscription;

        public GameState(IMediator mediator, ICachedStorageService storage, ISettingsService settingsService, ILaunchHistory launchHistory, ISnackbarService alert, ISerialStateContext serialContext, INavigationService nav)
        {
            _directoryState = new DirectoryState(storage);
            _mediator = mediator;
            _storage = storage;
            _settingsService = settingsService;
            _launchHistory = launchHistory;
            _alert = alert;

            _settingsSubscription = _settingsService.Settings
                .Do(settings => _settings = settings)
                .CombineLatest(serialContext.CurrentState, nav.SelectedNavigationView, (settings, serial, navView) => (settings, serial, navView))
                .Where(state => state.serial is SerialConnectedState)
                .Where(state => state.navView?.Type == NavigationLocation.Games)
                .Select(state => (path: state.settings.GetLibraryPath(TeensyLibraryType.Programs), state.settings.TargetType))
                .DistinctUntilChanged()
                .Select(storage => storage.path)
                .Do(state => _directoryState.ResetDirectoryTree(_settings!.GetLibraryPath(TeensyLibraryType.Programs)))
                .Subscribe(async path => await LoadDirectory(path));

            storage.DirectoryUpdated
                .Where(path => path.Equals(_directoryState.GetCurrentPath()))
                .Subscribe(async _ => await RefreshDirectory(bustCache: false));
        }

        public Task RefreshDirectory(bool bustCache = true)
        {
            _searchEnabled.OnNext(false);
            return _directoryState.RefreshDirectory(bustCache);
        }
        public Task LoadDirectory(string path, string? filePathToSelect = null)
        {
            _searchEnabled.OnNext(false);
            return _directoryState.LoadDirectory(path, filePathToSelect);
        }

        public Unit ToggleShuffleMode()
        {
            if (_gameMode.Value == NextPreviousMode.Shuffle)
            {
                _gameMode.OnNext(NextPreviousMode.Next);
                return Unit.Default;
            }
            _gameMode.OnNext(NextPreviousMode.Shuffle);
            return Unit.Default;
        }

        public Unit SetSelectedGame(GameItem game) 
        {
            _selectedGame.OnNext(game);
            return Unit.Default;
        }

        public async Task<bool> LoadGame(GameItem game, bool clearHistory = true)
        {
            if (clearHistory)
            {
                _launchHistory.Clear();
                _gameMode.OnNext(NextPreviousMode.Next);
            }

            _gameLaunched.OnNext(game);
            var result = await _mediator.Send(new LaunchFileCommand { Path = game.Path }); 

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
            _playState.OnNext(GameStateType.Playing);

            return true;
        }

        public async Task<bool> SaveFavorite(GameItem game)
        {
            var favGame = await _storage.SaveFavorite(game);
            var gameParentDir = favGame?.Path.GetUnixParentPath();

            if (gameParentDir is null) return false;

            var directoryResult = await _storage.GetDirectory(gameParentDir);

            if (directoryResult is null) return false;

            _directoryState.UpdateDirectory(directoryResult);

            return true;
        }

        public async Task<GameStateType> ToggleGame()
        {
            var playState = GetToggledPlayState();

            if (playState == GameStateType.Playing)
            {
                _gameLaunched.OnNext(_runningGame.Value.Clone());
                await _mediator.Send(new LaunchFileCommand { Path = _runningGame.Value.Path });
            }
            else
            {
                await _mediator.Send(new ResetCommand());
            }
            _playState.OnNext(playState);
            return playState;
        }

        public Task PlayPrevious()
        {
            return _gameMode.Value switch
            {
                NextPreviousMode.Next => PlayPreviousInDirectory(),
                NextPreviousMode.Shuffle => PlayPreviousShuffle(),
                _ => throw new NotImplementedException()
            };
        }

        private async Task PlayPreviousInDirectory()
        {
            var parentPath = _runningGame.Value.Path.GetUnixParentPath();
            var directoryResult = await _storage.GetDirectory(parentPath);

            if (directoryResult is null)
            {
                await LoadGame(_runningGame.Value);
                return;
            }
            var gameIndex = directoryResult.Files.IndexOf(_runningGame.Value);

            var gameToLoad = gameIndex == 0
                ? directoryResult.Files.Last() as GameItem
                : directoryResult.Files[--gameIndex] as GameItem;

            await LoadGame(gameToLoad!);
        }

        public async Task PlayPreviousShuffle()
        {
            var game = _launchHistory.GetPrevious(TeensyFileType.Prg, TeensyFileType.Crt) as GameItem;

            if (game is not null)
            {
                await LoadDirectory(game.Path.GetUnixParentPath());
                await LoadGame(game, clearHistory: false);
                return;
            }
            await LoadGame(_runningGame.Value, clearHistory: false);
            return;
        }

        public Task PlayNext()
        {
            return _gameMode.Value switch
            {
                NextPreviousMode.Next => PlayNextInDirectory(),
                NextPreviousMode.Shuffle => PlayNextShuffle(),
                _ => throw new NotImplementedException()
            };
        }

        private async Task PlayNextInDirectory()
        {
            if (_searchEnabled.Value) 
            {
                await PlayNextInSearch();
                return;
            }

            if (_runningGame.Value == null)
            {
                await PlayRandom();
                return;
            }
            var parentPath = _runningGame.Value.Path.GetUnixParentPath();
            var directoryResult = await _storage.GetDirectory(parentPath);

            if (directoryResult is null)
            {
                await LoadGame(_runningGame.Value);
                return;
            }
            var currentIndex = directoryResult.Files.IndexOf(_runningGame.Value);

            var file = directoryResult.Files.Count == currentIndex + 1
                ? directoryResult.Files.First()
                : directoryResult.Files[++currentIndex];

            if (file is GameItem game)
            {
                await LoadGame(game);
                return;
            }
            await LoadGame(_runningGame.Value);
        }

        private async Task PlayNextInSearch()
        {
            var currentItem = _searchResults.FirstOrDefault(i => i.Path.Equals(_runningGame.Value.Path));
            var currentIndex = _searchResults.IndexOf(currentItem);

            var file = _searchResults.Count == currentIndex + 1
                ? _searchResults.First()
                : _searchResults[++currentIndex];

            if (file is GameItem game)
            {
                await LoadGame(game);
                return;
            }
            await LoadGame(_runningGame.Value);
        }

        private async Task PlayNextShuffle()
        {
            if(_searchEnabled.Value) _searchEnabled.OnNext(false);

            var game = _launchHistory.GetNext(TeensyFileType.Crt, TeensyFileType.Prg) as GameItem;

            if (game is not null)
            {
                await LoadDirectory(game.Path.GetUnixParentPath());
                await LoadGame(game, clearHistory: false);
                return;
            }
            var newGame = await PlayRandom();

            if (newGame is null) return;

            return;
        }
        

        private GameStateType GetToggledPlayState() => _playState.Value == GameStateType.Playing
                ? GameStateType.Stopped
                : GameStateType.Playing;

        public async Task DeleteFile(GameItem game)
        {
            await _storage.DeleteFile(game, _settings.TargetType);
            await RefreshDirectory(bustCache: false);
        }

        public async Task<GameItem?> PlayRandom()
        {            
            var game = _storage.GetRandomFile(TeensyFileType.Crt, TeensyFileType.Prg) as GameItem;

            if (game is not null)
            {
                await LoadDirectory(game.Path.GetUnixParentPath(), game.Path);
                await LoadGame(game, clearHistory: false);

                if (_gameMode.Value != NextPreviousMode.Shuffle) ToggleShuffleMode();

                _launchHistory.Add(game!);

                return game;
            }
            _alert.Enqueue("Random search requires visiting at least one directory with programs in it first.  Try the cache button next to the dice for best results.");
            return null;
        }

        public Unit SearchGames(string searchText)
        {
            var searchResult = _storage.SearchPrograms(searchText);

            if (searchResult is null) return Unit.Default;

            if (_gameMode.Value == NextPreviousMode.Shuffle) ToggleShuffleMode();

            _searchResults = searchResult.ToList();

            _directoryState.SetSearchResults(searchResult);
            _searchEnabled.OnNext(true);
            return Unit.Default;
        }

        public Task ClearSearch()
        {
            _searchEnabled.OnNext(false);
            return _directoryState.ClearSearchResults();            
        }
        public Task CacheAll() => _storage.CacheAll();
        public Task NextPage() => _directoryState.GoToNextPage();
        public Task PreviousPage() => _directoryState.GoToPreviousPage();
        public Task SetPageSize(int pageSize) => _directoryState.SetPageSize(pageSize);

        public void Dispose()
        {
            _settingsSubscription?.Dispose();
            _currentTimeSubscription?.Dispose();
        }
    }
}