using MediatR;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows;
using System.Windows.Input;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Commands.File.LaunchFile;
using TeensyRom.Core.Common;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Core.Storage.Services;
using TeensyRom.Ui.Controls.DirectoryTree;
using TeensyRom.Ui.Controls.PlayToolbar;
using TeensyRom.Ui.Features.Common.Config;
using TeensyRom.Ui.Features.Common.Models;
using TeensyRom.Ui.Features.Common.State;
using TeensyRom.Ui.Features.Common.State.Directory;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Services;
using SystemDirectory = System.IO.Directory;

namespace TeensyRom.Ui.Features.Common.State.Player
{
    public abstract class PlayerContext : IPlayerContext
    {
        public IObservable<string> CurrentPath => _directoryState.Select(d => d.CurrentPath);
        public IObservable<PlayerState> CurrentState => _currentState.AsObservable();
        public IObservable<int> CurrentPage => _directoryState.Select(d => d.CurrentPage);
        public IObservable<int> TotalPages => _directoryState.Select(d => d.TotalPages);
        public IObservable<bool> PagingEnabled => _directoryState.Select(d => d.PagingEnabled);
        public IObservable<DirectoryNodeViewModel?> DirectoryTree => _tree.DirectoryTree.AsObservable();
        public IObservable<ObservableCollection<IStorageItem>> DirectoryContent => _directoryState.Select(d => d.DirectoryContent);
        public IObservable<ILaunchableItem> LaunchedFile => _launchedFile.AsObservable();
        public IObservable<ILaunchableItem> SelectedFile => _selectedFile.AsObservable();
        public IObservable<PlayState> PlayingState => _playingState.AsObservable();

        private string _currentPath = string.Empty;

        protected PlayerState? _previousState;
        protected readonly BehaviorSubject<PlayerState> _currentState;        
        protected readonly BehaviorSubject<ILaunchableItem> _launchedFile = new(null!);
        protected readonly BehaviorSubject<ILaunchableItem> _selectedFile = new(null!);
        protected readonly BehaviorSubject<PlayState> _playingState = new(PlayState.Stopped);
        protected BehaviorSubject<DirectoryState> _directoryState = new(new());

        protected IDisposable? _settingsSubscription;
        protected TeensySettings _settings = null!;
        protected readonly IMediator _mediator;
        protected readonly ICachedStorageService _storage;
        protected readonly ISettingsService _settingsService;
        protected readonly ILaunchHistory _launchHistory;
        protected readonly ISnackbarService _alert;
        protected readonly ISerialStateContext _serialContext;
        protected readonly INavigationService _nav;
        protected readonly IDirectoryTreeState _tree;
        protected readonly IExplorerViewConfig _config;
        protected readonly Dictionary<Type, PlayerState> _states;
        protected readonly List<IDisposable> _stateSubscriptions = new();
        protected TeensyLibrary _currentLibrary;

        public PlayerContext(IMediator mediator, ICachedStorageService storage, ISettingsService settingsService, ILaunchHistory launchHistory, ISnackbarService alert, ISerialStateContext serialContext, INavigationService nav, IDirectoryTreeState tree, IExplorerViewConfig config)
        {
            _mediator = mediator;
            _storage = storage;
            _settingsService = settingsService;
            _launchHistory = launchHistory;
            _alert = alert;
            _serialContext = serialContext;
            _nav = nav;
            _tree = tree;
            _config = config;
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
            _settingsService.Settings
                .Where(settings => settings is not null)
                .Take(1)
                .Subscribe(settings => _currentLibrary = settings.Libraries.First(lib => lib.Type == _config.LibraryType));

            _settingsSubscription = _settingsService.Settings                
                .Do(settings => _settings = settings)
                .CombineLatest(_serialContext.CurrentState, _nav.SelectedNavigationView, (settings, serial, navView) => (settings, serial, navView))
                .Where(state => state.serial is SerialConnectedState)
                .Where(state => state.navView?.Type == _config.NavigationLocation)
                .DistinctUntilChanged(state => string.Join("", state.settings.Libraries.Select(lib => lib.Path)))
                .Select(state => (path: _currentLibrary.Path, state.settings.TargetType))
                .Select(storage => storage.path)
                .Do(path => _tree.ResetDirectoryTree(path))                
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
        public Task LoadDirectory(string path)
        {
            return LoadDirectory(path, null);
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
            _directoryState.Value.LoadDirectory(cacheItem.ToList(), cacheItem.Path, filePathToSelect);

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

        public virtual async Task TogglePlay()
        {
            if (_playingState.Value is PlayState.Playing)
            {
                _playingState.OnNext(PlayState.Stopped);
                await StopFile();                
                return;
            }
            _playingState.OnNext(PlayState.Playing);

            if(_launchedFile.Value is GameItem)
            {
                await PlayFile(_launchedFile.Value);
                return;
            }
            await _mediator.Send(new ToggleMusicCommand());
        }

        public virtual async Task PlayFile(ILaunchableItem file)
        {
            if(file is null) return;

            if (!file.IsCompatible) 
            {
                await HandleBadFile(file);
                return;
            }

            var result = await _mediator.Send(new LaunchFileCommand { Path = file.Path });

            if (result.LaunchResult is LaunchFileResultType.ProgramError or LaunchFileResultType.SidError)
            {
                await HandleBadFile(file);
                return;
            }
            Application.Current.Dispatcher.Invoke(() =>
            {
                file.IsSelected = true;

                var shouldUpdateCurrent = _launchedFile.Value is not null
                    && file.Path.Equals(_launchedFile.Value.Path) == false;

                if (shouldUpdateCurrent)
                {
                    _launchedFile.Value!.IsSelected = false;
                }
            });
            _launchedFile.OnNext(file);
            _selectedFile.OnNext(file);
            _playingState.OnNext(PlayState.Playing);
        }

        public Task HandleBadFile(ILaunchableItem file)
        {
            _alert.Enqueue($"{file.Name} is currently unsupported (see logs).  Skipping to the next file.");

            _launchedFile.OnNext(file); //play next will use this to determine the next file

            if(file.IsCompatible is true) _storage.MarkIncompatible(file);
            
            return PlayNext();
        }
        
        public virtual async Task SaveFavorite(ILaunchableItem file)
        {
            var favFile = await _storage.SaveFavorite(file);
            var parentDir = favFile?.Path.GetUnixParentPath();

            if (parentDir is null) return;

            var directoryResult = await _storage.GetDirectory(parentDir);

            if (directoryResult is null) return;

            _directoryState.Value.LoadDirectory(directoryResult.ToList(), directoryResult.Path);
        }

        public Task DeleteFile(IFileItem file) 
        {
            if (_directoryState.Value is null) return Task.CompletedTask;

            _storage.DeleteFile(file, _settings.TargetType);

            _storage.ClearCache(_directoryState.Value.CurrentPath);

            return LoadDirectory(_directoryState.Value.CurrentPath);
        }
        public async Task PlayNext()
        {
            var file = await _currentState.Value.GetNext(_launchedFile.Value, _currentLibrary.Type, _directoryState.Value);

            if (file is null)
            {
                _alert.Enqueue("Random search requires visiting at least one directory with files in it first.  Try clicking the cache all button.");
                return;
            }
            await PlayFile(file);
        }
        public async virtual Task PlayPrevious()
        {
            var file = await _currentState.Value.GetPrevious(_launchedFile.Value, _currentLibrary.Type, _directoryState.Value);

            if (file is not null) await PlayFile(file);
        }
        public Task StopFile() 
        {
            _playingState.OnNext(PlayState.Stopped);

            if(_launchedFile.Value is GameItem)
            {
                return _mediator.Send(new ResetCommand());
            }
            return _mediator.Send(new ToggleMusicCommand());
        }

        public async Task<ILaunchableItem?> PlayRandom()
        {
            var success = TryTransitionTo(typeof(ShuffleState));

            if(!success) return _launchedFile.Value;

            _launchHistory.ClearForwardHistory();
            await PlayNext();
            return _launchedFile.Value;
        }

        public void UpdateHistory(ILaunchableItem fileToLoad)
        {
            var isNotConsecutiveDuplicate = _launchedFile.Value is null 
                || !fileToLoad.Path.Equals(_launchedFile.Value.Path, StringComparison.OrdinalIgnoreCase);

            if (isNotConsecutiveDuplicate) _launchHistory.Add(fileToLoad);
        }
        public Unit SearchFiles(string searchText)
        {
            var success = TryTransitionTo(typeof(SearchState));

            if (!success) return Unit.Default;

            var searchResult = _storage.Search(searchText, GetFileTypes())
                .Where(f => f.Path.Contains(_currentLibrary.Path))
                .Cast<IStorageItem>()
                .ToList();

            if (searchResult is null) return Unit.Default;
                
            _directoryState.Value.ClearSelection();
            _directoryState.Value.LoadDirectory(searchResult, "Search Results:");
            var firstItem = _directoryState.Value.SelectFirst();
               
            if(firstItem is not null) _selectedFile.OnNext(firstItem);

            _directoryState.OnNext(_directoryState.Value);

            return Unit.Default;
        }

        public async Task ClearSearch()
        {
            _directoryState.Value.ClearSelection();
            await LoadDirectory(_currentPath);            
            var firstItem = _directoryState.Value.SelectFirst();

            if (firstItem is not null)
            {
                _selectedFile.OnNext(firstItem);
            }
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
        public Unit SelectFile(ILaunchableItem file)
        {
            file.IsSelected = true;

            var shouldUpdateCurrent = _selectedFile.Value is not null
                && file.Path.Equals(_selectedFile.Value.Path) == false;

            if (shouldUpdateCurrent)
            {
                _selectedFile.Value!.IsSelected = false;
            }
            _selectedFile.OnNext(file);
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

        public Task SwitchLibrary(TeensyLibrary library)
        {
            _currentLibrary = library;
            return Task.CompletedTask;        
        }

        public TeensyFileType[] GetFileTypes() 
        {
            if (_currentLibrary.Type == TeensyLibraryType.All) 
            {
                return _settings.FileTargets
                    .Where(ft => ft.Type != TeensyFileType.Hex)
                    .Select(t => t.Type).ToArray();
            }
            return _settings.FileTargets
                .Where(ft => ft.LibraryType == _currentLibrary.Type)
                .Select(t => t.Type).ToArray();            
        }

        public async Task StoreFiles(IEnumerable<FileCopyItem> files)
        {
            var commonParent = files.Select(i => i.Path).GetCommonBasePath();
            var directoryOnly = files.All(i => i.InSubdirectory);

            foreach (var file in files)
            {
                var fileInfo = new TeensyFileInfo(file.Path);

                var finalPath = directoryOnly
                    ? SystemDirectory.GetParent(commonParent)?.FullName
                    : commonParent;

                var relativePath = fileInfo.FullPath
                    .Replace(finalPath!, string.Empty)
                    .ToUnixPath()
                    .GetUnixParentPath();

                fileInfo.TargetPath = _directoryState.Value
                    .CurrentPath
                    .UnixPathCombine(relativePath);

                await _storage.SaveFile(fileInfo);
            }
            await RefreshDirectory(bustCache: false);
        }        
    }
}