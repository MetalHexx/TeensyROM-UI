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

namespace TeensyRom.Ui.Features.Games.State
{
    public class GameState : IGameState, IDisposable
    {
        public IObservable<DirectoryNodeViewModel> DirectoryTree => _directoryTree.AsObservable();
        public IObservable<ObservableCollection<StorageItem>> DirectoryContent => _directoryContent.AsObservable();
        public IObservable<GameItem> RunningGame => _runningGame.AsObservable();
        public IObservable<GameItem> SelectedGame => _selectedGame.AsObservable();
        public IObservable<GameMode> CurrentGameMode => _gameMode.AsObservable();
        public IObservable<GameStateType> CurrentPlayState => _playState.AsObservable();
        public IObservable<GameItem> GameLaunched => _gameLaunched.AsObservable();

        private readonly BehaviorSubject<DirectoryNodeViewModel> _directoryTree = new(new());
        private readonly Subject<ObservableCollection<StorageItem>> _directoryContent = new();
        private readonly BehaviorSubject<StorageCacheItem?> _currentDirectory = new(null);
        private readonly BehaviorSubject<GameItem> _runningGame = new(null);
        private readonly BehaviorSubject<GameItem> _selectedGame = new(null);
        private readonly BehaviorSubject<GameMode> _gameMode = new(GameMode.Next);
        private readonly BehaviorSubject<GameStateType> _playState = new(GameStateType.Paused);
        private Subject<GameItem> _gameLaunched = new();

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
            _mediator = mediator;
            _storage = storage;
            _settingsService = settingsService;
            _launchHistory = launchHistory;
            _alert = alert;
            _settingsService.Settings.Subscribe(OnSettingsChanged);

            _settingsSubscription = _settingsService.Settings
                .Do(settings => _settings = settings)
                .CombineLatest(serialContext.CurrentState, nav.SelectedNavigationView, (settings, serial, navView) => (settings, serial, navView))
                .Where(state => state.serial is SerialConnectedState)
                .Where(state => state.navView?.Type == NavigationLocation.Games)
                .Select(state => (path: state.settings.GetLibraryPath(TeensyLibraryType.Programs), state.settings.TargetType))
                .DistinctUntilChanged()
                .Select(storage => storage.path)
                .Do(_ => ResetDirectoryTree())
                .Subscribe(async path => await LoadDirectory(path));

            storage.DirectoryUpdated
                .Where(path => path.Equals(_currentDirectory.Value?.Path))
                .Subscribe(async _ => await RefreshDirectory(bustCache: false));
        }

        private void OnSettingsChanged(TeensySettings settings)
        {
            _settings = settings;
            _directoryContent.OnNext(new ObservableCollection<StorageItem>());
            ResetDirectoryTree();
        }

        private void ResetDirectoryTree()
        {
            var programLibraryPath = _settings.GetLibraryPath(TeensyLibraryType.Programs);

            var dirItem = new DirectoryNodeViewModel
            {
                Name = "Fake Root",  //TODO: Fake root required since UI view binds to enumerable -- design could use improvement
                Path = "Fake Root",
                Directories =
                [
                    new DirectoryNodeViewModel
                    {
                        Name = programLibraryPath,
                        Path = programLibraryPath,
                        Directories = []
                    }
                ]
            };
            _directoryTree.OnNext(dirItem);
        }

        public Unit ToggleShuffleMode()
        {
            if (_gameMode.Value == GameMode.Shuffle)
            {
                _gameMode.OnNext(GameMode.Next);
                return Unit.Default;
            }
            _gameMode.OnNext(GameMode.Shuffle);
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
                _gameMode.OnNext(GameMode.Next);
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

        private string GetGameArtPath(GameItem game, string parentPath) => Path.Combine(parentPath, game.Name.ReplaceExtension(".png"));

        public async Task<bool> SaveFavorite(GameItem game)
        {
            var favGame = await _storage.SaveFavorite(game);
            var gameParentDir = favGame?.Path.GetUnixParentPath();

            if (gameParentDir is null) return false;

            var directoryResult = await _storage.GetDirectory(gameParentDir);

            if (directoryResult is null) return false;

            Application.Current.Dispatcher.Invoke(() =>
            {
                _directoryTree.Value.Insert(directoryResult.Directories);
            });

            _directoryTree.OnNext(_directoryTree.Value);

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
                GameMode.Next => PlayPreviousInDirectory(),
                GameMode.Shuffle => PlayPreviousShuffle(),
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
                GameMode.Next => PlayNextInDirectory(),
                GameMode.Shuffle => PlayNextShuffle(),
                _ => throw new NotImplementedException()
            };
        }

        private async Task PlayNextInDirectory()
        {
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

        private async Task PlayNextShuffle()
        {
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

        public async Task LoadDirectory(string path)
        {
            var directoryResult = await _storage.GetDirectory(path);

            if (directoryResult is null)
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                _directoryTree.Value.Insert(directoryResult.Directories);
                _directoryTree.Value.SelectDirectory(path);
            });

            _directoryTree.OnNext(_directoryTree.Value);

            var directoryItems = new ObservableCollection<StorageItem>();
            directoryItems.AddRange(directoryResult.Directories);
            directoryItems.AddRange(directoryResult.Files);

            _directoryContent.OnNext(directoryItems);
            _currentDirectory.OnNext(directoryResult);
            _directoryTree.OnNext(_directoryTree.Value);

            return;
        }

        private GameStateType GetToggledPlayState() => _playState.Value == GameStateType.Playing
                ? GameStateType.Paused
                : GameStateType.Playing;

        public async Task RefreshDirectory(bool bustCache = true)
        {
            if (_currentDirectory.Value is null) return;

            if (bustCache) _storage.ClearCache(_currentDirectory.Value.Path);

            await LoadDirectory(_currentDirectory.Value.Path);
        }

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
                await LoadDirectory(game.Path.GetUnixParentPath());
                await LoadGame(game, clearHistory: false);

                if (_gameMode.Value != GameMode.Shuffle) ToggleShuffleMode();

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

            _directoryContent.OnNext(new ObservableCollection<StorageItem>(searchResult));
            return Unit.Default;
        }

        public Task ClearSearch()
        {
            return LoadDirectory(_currentDirectory.Value?.Path ?? "/");
        }

        public Task CacheAll() => _storage.CacheAll();

        public void Dispose()
        {
            _settingsSubscription?.Dispose();
            _currentTimeSubscription?.Dispose();
        }
    }
}