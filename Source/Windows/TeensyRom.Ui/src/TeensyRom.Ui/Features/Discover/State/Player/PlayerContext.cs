using DynamicData.Kernel;
using MediatR;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Runtime;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows;
using System.Windows.Input;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Commands.File.LaunchFile;
using TeensyRom.Core.Commands.PlaySubtune;
using TeensyRom.Core.Common;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Music.Sid;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Core.Storage.Services;
using TeensyRom.Core.Storage.Tools.D64Extraction;
using TeensyRom.Core.Storage.Tools.Zip;
using TeensyRom.Ui.Controls.DirectoryTree;
using TeensyRom.Ui.Controls.PlayToolbar;
using TeensyRom.Ui.Features.Common.Config;
using TeensyRom.Ui.Features.Common.Models;
using TeensyRom.Ui.Features.Discover.State;
using TeensyRom.Ui.Features.Discover.State.Directory;
using TeensyRom.Ui.Core.Progress;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Services;
using SystemDirectory = System.IO.Directory;
using TeensyRom.Core.Commands.SetMusicSpeed;
using TeensyRom.Core.Music;

namespace TeensyRom.Ui.Features.Discover.State.Player
{
    public class PlayerContext : IPlayerContext
    {
        public IObservable<string> CurrentPath => _directoryState.Select(d => d.CurrentPath);
        public IObservable<PlayerState> CurrentState => _currentState.AsObservable();
        public IObservable<int> CurrentPage => _directoryState.Select(d => d.CurrentPage);
        public IObservable<int> TotalPages => _directoryState.Select(d => d.TotalPages);
        public IObservable<bool> PagingEnabled => _directoryState.Select(d => d.PagingEnabled);
        public IObservable<DirectoryNodeViewModel?> DirectoryTree => _tree.DirectoryTree.AsObservable();
        public IObservable<ObservableCollection<IStorageItem>> DirectoryContent => _directoryState.Select(d => d.DirectoryContent);
        public IObservable<LaunchedFileResult> LaunchedFile => _launchedFile.AsObservable();
        public IObservable<ILaunchableItem> SelectedFile => _selectedFile.AsObservable();
        public IObservable<PlayState> PlayingState => _playingState.AsObservable();
        public IObservable<StorageScope> CurrentScope => _currentScope.AsObservable();
        public IObservable<string> CurrentScopePath => _currentScopePath.AsObservable();

        private string _currentPath = string.Empty;
        private string _previousSearch = string.Empty;
        private TimeSpan _currentTime = TimeSpan.Zero;        

        protected PlayerState? _previousState;
        protected readonly BehaviorSubject<PlayerState> _currentState;
        protected readonly BehaviorSubject<LaunchedFileResult> _launchedFile = new(null!);
        protected readonly BehaviorSubject<ILaunchableItem> _selectedFile = new(null!);
        protected readonly BehaviorSubject<PlayState> _playingState = new(PlayState.Stopped);
        protected readonly BehaviorSubject<StorageScope> _currentScope = new(StorageScope.Storage);
        protected readonly BehaviorSubject<string> _currentScopePath = new(StorageConstants.Remote_Path_Root);
        protected BehaviorSubject<DirectoryState> _directoryState = new(new());

        protected IDisposable? _settingsSubscription;
        protected TeensySettings _settings = null!;
        protected readonly IMediator _mediator;
        protected readonly ICachedStorageService _storage;
        protected readonly ISettingsService _settingsService;
        protected readonly ILaunchHistory _launchHistory;
        private readonly ID64Extractor _d64Extractor;
        private readonly IZipExtractor _zipExtractor;
        protected readonly ISnackbarService _alert;
        protected readonly ISerialStateContext _serial;
        protected readonly INavigationService _nav;
        protected readonly IDiscoveryTreeState _tree;
        private readonly ILoggingService _log;
        protected readonly Dictionary<Type, PlayerState> _states;
        protected readonly List<IDisposable> _stateSubscriptions = new();
        protected TeensyFilter _currentFilter;

        public PlayerContext(IMediator mediator, ICachedStorageService storage, ISettingsService settingsService, ILaunchHistory launchHistory, IFileWatchService watchService, ID64Extractor d64Extractor, IZipExtractor zipExtractor, ISnackbarService alert, ISerialStateContext serialContext, INavigationService nav, IDiscoveryTreeState tree, IProgressTimer timer, ILoggingService log)
        {
            _mediator = mediator;
            _storage = storage;
            _settingsService = settingsService;
            _launchHistory = launchHistory;
            _d64Extractor = d64Extractor;
            _zipExtractor = zipExtractor;
            _alert = alert;
            _serial = serialContext;
            _nav = nav;
            _tree = tree;
            _log = log;
            _states = new()
            {
                { typeof(NormalPlayState), new NormalPlayState(this, mediator, storage, settingsService, launchHistory, alert, serialContext, nav, tree) },
                { typeof(ShuffleState), new ShuffleState(this, mediator, storage, settingsService, launchHistory, alert, serialContext, nav, tree) },
                { typeof(SearchState), new SearchState(this, mediator, storage, settingsService, launchHistory, alert, serialContext, nav, tree) }
            };
            _currentState = new(_states[typeof(NormalPlayState)]);

            SubscribeToStateObservables();

            storage.FilesAdded
                .SubscribeOn(RxApp.MainThreadScheduler)
                .Subscribe(async files =>
                {
                    await UpdateDirectoryTree(files.ToList());

                    var fileToAutoLaunch = files.OfType<ILaunchableItem>().FirstOrDefault();

                    if (fileToAutoLaunch is not null && _settings.AutoLaunchOnCopyEnabled)
                    {
                        await PlayFile(fileToAutoLaunch);
                    }
                });

            watchService.WatchFiles
                .SubscribeOn(RxApp.MainThreadScheduler)
                .Where(files => files is not null && files.Any())
                .Subscribe(async files => await AutoStoreFiles(files));

            timer.CurrentTime.Subscribe(time => _currentTime = time);
        }

        private void SubscribeToStateObservables()
        {
            _settingsService.Settings
                .Where(settings => settings is not null)
                .Take(1)
                .Subscribe(settings => _currentFilter = settings.FileFilters.First(lib => lib.Type == TeensyFilterType.All));

            _settingsSubscription = _settingsService.Settings
                .Do(settings => _settings = settings)
                .Do(settings => _currentFilter = _settings.GetStartupFilter())
                .CombineLatest(_serial.CurrentState, _nav.SelectedNavigationView, (settings, serial, navView) => (settings, serial, navView))
                .Where(state => state.navView?.Type == NavigationLocation.Discover && state.serial is SerialConnectedState)
                .DistinctUntilChanged(state => state.settings.StorageType)
                .Select(state => (path: _currentFilter, state.settings.StorageType))
                .Select(storage => storage.path)
                .Do(path => _tree.ResetDirectoryTree(StorageConstants.Remote_Path_Root))
                .Subscribe(async path => await LoadDirectory(StorageConstants.Remote_Path_Root));

            _serial.CurrentState
                .Where(_ => _settings.StartupLaunchEnabled && _settings.FirstTimeSetup == false)
                .OfType<SerialConnectedState>()
                .Take(1)
                .Subscribe(async _ => await SwitchFilter(_settings.GetStartupFilter()));
        }

        private async Task UpdateDirectoryTree(List<IFileItem> files)
        {
            var uniquePaths = files
                .Select(f => f.Path.GetUnixParentPath().RemoveLeadingAndTrailingSlash())
                .Distinct();

            await Application.Current.Dispatcher.Invoke(async () =>
            {
                foreach (var path in uniquePaths)
                {
                    List<string> directoryParts = ["/"];

                    directoryParts.AddRange(path.ToPathArray());

                    var currentDir = string.Empty;

                    List<DirectoryItem> dirsToAdd = [];

                    foreach (var directory in directoryParts.Take(2))
                    {
                        currentDir = currentDir.UnixPathCombine(directory);
                        var cacheItem = await _storage.GetDirectory(currentDir);

                        if (cacheItem is not null)
                        {
                            _tree.Insert(cacheItem.Directories);
                        }
                    }
                }
                await LoadDirectory(_directoryState.Value.CurrentPath);
            });
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
                if (_previousState is not null)
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
        public async Task RefreshDirectory(bool bustCache = true)
        {
            var success = TryTransitionTo(typeof(NormalPlayState));

            if (success)
            {
                if (string.IsNullOrWhiteSpace(_currentPath)) return;

                if (bustCache)
                {
                    _storage.ClearCache(_currentPath);
                    await _storage.CacheAll(_currentPath);
                }
                await LoadDirectory(_currentPath);
            }
        }

        public async Task TogglePlay()
        {
            if (_launchedFile.Value.File is SongItem)
            {
                var result = await _mediator.Send(new ToggleMusicCommand());

                if (result.IsBusy)
                {
                    _alert.Enqueue("Toggle music failed. Re-launching the current song.");
                    _playingState.OnNext(PlayState.Playing);
                    await PlayFile(_launchedFile.Value!.File);
                    return;
                }
                _playingState.OnNext(_playingState.Value == PlayState.Paused
                    ? PlayState.Playing
                    : PlayState.Paused);
                return;
            }
            if (_playingState.Value is PlayState.Playing)
            {
                _playingState.OnNext(PlayState.Stopped);
                await StopFile();
                return;
            }
            _playingState.OnNext(PlayState.Playing);
            await PlayFile(_launchedFile.Value!.File);
            return;
        }

        public virtual async Task PlayFile(ILaunchableItem file)
        {
            if (file is null) return;

            if (!file.IsCompatible)
            {
                await HandleBadFile(file);
                return;
            }
            if (IsBusy()) return;

            LaunchFileAsync(file);

            Application.Current.Dispatcher.Invoke(() =>
            {
                file.IsSelected = true;

                var shouldUpdateCurrent = _launchedFile.Value?.File is not null
                    && file.Path.Equals(_launchedFile.Value?.File.Path) == false;

                if (shouldUpdateCurrent)
                {
                    _launchedFile.Value!.File.IsSelected = false;
                }
            });
            _launchedFile.OnNext(GetLaunchResult(file));
            _selectedFile.OnNext(file);
            _playingState.OnNext(PlayState.Playing);
        }

        private LaunchedFileResult GetLaunchResult(ILaunchableItem file) 
        {
            var isRandom = _currentState.Value is ShuffleState && _launchHistory.CurrentIsNew && file.Path != _launchedFile.Value?.File.Path;
            return new LaunchedFileResult(file, isRandom);
        }
        private bool IsBusy()
        {
            var serialState = _serial.CurrentState.FirstOrDefault();

            return serialState is SerialBusyState;
        }

        private void LaunchFileAsync(ILaunchableItem file)
        {
            Task.Run(() =>
            {
                _mediator
                .Send(new LaunchFileCommand(_settings.StorageType, file))
                .ContinueWith(async task =>
                {
                    var result = task.Result;
                    if (result.LaunchResult is LaunchFileResultType.ProgramError or LaunchFileResultType.SidError)
                    {
                        await Application.Current.Dispatcher.BeginInvoke(new Action(async () =>
                        {
                            await HandleBadFile(file);
                        }));
                    }
                });
            });
        }

        public Task HandleBadFile(ILaunchableItem file)
        {
            _alert.Enqueue($"{file.Name} is currently unsupported (see logs).  Skipping to the next file.");

            _launchedFile.OnNext(GetLaunchResult(file)); //play next will use this to determine the next file            

            if (file.IsCompatible is true) _storage.MarkIncompatible(file);

            _launchHistory.Remove(file);

            return PlayNext();
        }

        public virtual async Task SaveFavorite(ILaunchableItem file)
        {
            if (IsBusy()) return;

            if (_launchedFile.Value?.File is GameItem)
            {
                _alert.Enqueue("Your game will re-launch to allow favorite to be tagged.");
                await _mediator.Send(new ResetCommand());
            }
            var favFile = await _storage.SaveFavorite(file);

            if (_launchedFile?.Value?.File is GameItem)
            {
                await Task.Delay(2000);
                await _mediator.Send(new LaunchFileCommand(_settings.StorageType, _launchedFile.Value.File));
            }
            if (_currentState.Value is not SearchState) 
            {
                await LoadDirectory(_directoryState.Value.CurrentPath, file.Path);
            }            
        }

        public async Task RemoveFavorite(ILaunchableItem file)
        {
            if (IsBusy()) return;

            var isGameItem = _launchedFile.Value?.File is GameItem;

            if (_launchedFile.Value?.File is GameItem)
            {
                _alert.Enqueue("Resetting TR to allow favorite to be untagged.");
                await _mediator.Send(new ResetCommand());
            }
            await _storage.RemoveFavorite(file);

            if(_currentState.Value is not SearchState)
            {
                await LoadDirectory(_directoryState.Value.CurrentPath, file.Path);
            }
        }

        public Task DeleteFile(IFileItem file)
        {
            if (_directoryState.Value is null) return Task.CompletedTask;

            _storage.DeleteFile(file, _settings.StorageType);

            _storage.ClearCache(_directoryState.Value.CurrentPath);

            return LoadDirectory(_directoryState.Value.CurrentPath);
        }
        public async Task PlayNext()
        {
            if (IsBusy()) return;

            var file = await _currentState.Value.GetNext(_launchedFile.Value?.File, _currentFilter.Type, _directoryState.Value);

            if (file is null) 
            {
                _alert.Enqueue("No file was found.  Check your filters and pinned directory.");
                await StopFile();
                return;
            }
            await PlayFile(file);
        }
        public async Task PlayPrevious()
        {
            if (IsBusy()) return;

            if (_launchedFile.Value.File is SongItem && _currentTime >= TimeSpan.FromSeconds(3))
            {
                await PlayFile(_launchedFile.Value!.File);
                return; 
            }
            if (_launchedFile.Value.File is ILaunchableItem)
            {
                var file = await _currentState.Value.GetPrevious(_launchedFile.Value.File, _currentFilter.Type, _directoryState.Value);

                if (file is not null) await PlayFile(file);

                return;
            }
            await PlayFile(_launchedFile.Value!.File);
            return;
        }
        public Task StopFile()
        {
            _playingState.OnNext(PlayState.Stopped);

            if (_launchedFile.Value?.File is SongItem)
            {
                return _mediator.Send(new ToggleMusicCommand());
            }
            return _mediator.Send(new ResetCommand());
        }

        public async Task<ILaunchableItem?> PlayRandom()
        {
            if (IsBusy()) return _launchedFile.Value?.File;

            var success = TryTransitionTo(typeof(ShuffleState));

            if (!success) return _launchedFile.Value?.File;

            _launchHistory.ClearForwardHistory();
            await PlayNext();
            return _launchedFile.Value?.File;
        }

        public void UpdateHistory(ILaunchableItem fileToLoad)
        {
            var isNotConsecutiveDuplicate = _launchedFile.Value?.File is null
                || !fileToLoad.Path.Equals(_launchedFile.Value.File.Path, StringComparison.OrdinalIgnoreCase);

            if (isNotConsecutiveDuplicate) _launchHistory.Add(fileToLoad);
        }
        public Unit SearchFiles(string searchText)
        {
            _previousSearch = searchText;

            var success = TryTransitionTo(typeof(SearchState));

            if (!success) return Unit.Default;

            var searchResult = _storage.Search(searchText, GetFileTypes())
                .Cast<IStorageItem>()
                .ToList();

            if (searchResult is null) return Unit.Default;

            _directoryState.Value.ClearSelection();
            _directoryState.Value.LoadDirectory(searchResult, "Search Results:");
            var firstItem = _directoryState.Value.SelectFirst();

            if (firstItem is not null) _selectedFile.OnNext(firstItem);

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
        public async Task CacheAll()
        {
            await _mediator.Send(new ResetCommand());
            await _storage.CacheAll();
            _tree.ResetDirectoryTree(StorageConstants.Remote_Path_Root);
            await LoadDirectory(StorageConstants.Remote_Path_Root);
        }
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

            if (_currentState.Value is not SearchState)
            {
                TryTransitionTo(typeof(NormalPlayState));
            }
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

        public async Task SwitchFilter(TeensyFilter filter)
        {
            _currentFilter = filter;

            if (_currentState.Value is SearchState)
            {
                SearchFiles(_previousSearch);
                return;
            }
            await PlayRandom();
        }

        public TeensyFileType[] GetFileTypes()
        {
            if (_currentFilter.Type == TeensyFilterType.All)
            {
                return _settings.FileTargets
                    .Where(ft => ft.Type != TeensyFileType.Hex)
                    .Select(t => t.Type).ToArray();
            }
            return _settings.FileTargets
                .Where(ft => ft.FilterType == _currentFilter.Type)
                .Select(t => t.Type).ToArray();
        }

        public async Task StoreFiles(IEnumerable<DragNDropFile> files)
        {
            var sourceCommonParent = files.Select(i => i.Path).GetCommonBasePath();
            var isDirectoryCopy = files.All(i => i.InSubdirectory);

            var sourceParentPath = isDirectoryCopy
                ? SystemDirectory.GetParent(sourceCommonParent)?.FullName
                : sourceCommonParent;

            var targetBasePath = _directoryState.Value.CurrentPath;

            var transferItems = files
                .Select(f =>
                {
                    var targetRelativePath = f.Path
                        .Replace(sourceParentPath!, string.Empty)
                        .ToUnixPath()
                        .GetUnixParentPath();

                    return new FileTransferItem
                    (
                        sourcePath: f.Path,
                        targetPath: targetBasePath.UnixPathCombine(targetRelativePath),
                        targetStorage: _settings.StorageType
                    );
                })
                .ToList();

            transferItems = ExtractZips(transferItems);
            transferItems = ExtractD64(transferItems);

            await _mediator.Send(new ResetCommand());

            var saveResults = await _storage.SaveFiles(transferItems);

            var fileCount = files.Count();

            if (saveResults.FailedFiles.Any())
            {
                _alert.Enqueue($"{saveResults.FailedFiles.Count} files had an error being copied. \r\n See Logs.");
                saveResults.FailedFiles.ForEach(d => _log.InternalError($"Error copying file: {d.SourcePath}"));
            }
            _alert.Enqueue($"{transferItems.Count() - saveResults.FailedFiles.Count} files were saved to the TR.");

            if (_settings.AutoLaunchOnCopyEnabled) return;

            await Application.Current.Dispatcher.Invoke(() =>
            {
                if (targetBasePath == _directoryState.Value.CurrentPath)
                {
                    return LoadDirectory(targetBasePath);
                }
                return Task.CompletedTask;
            });
        }

        private List<FileTransferItem> ExtractZips(List<FileTransferItem> files) 
        {
            var zipFiles = files.Where(file => new FileInfo(file.SourcePath).Extension.Contains("zip", StringComparison.OrdinalIgnoreCase));

            if (!zipFiles.Any())
            {
                return files;
            }
            var fileMessageString = zipFiles.Count() > 1 ? "files" : "file";
            _alert.Enqueue($"Unpacking ZIP {fileMessageString}.");

            var extractedFiles = zipFiles
                .Select(f => (extraction: _zipExtractor.Extract(f), zip: f))
                .Select(f => f.extraction.ExtractedFiles
                    .Select(fileInfo => new FileTransferItem
                    (
                        sourcePath: fileInfo.FullName,
                        targetPath: f.zip.TargetPath,
                        targetStorage: _settings.StorageType)
                    ))
                .SelectMany(f => f)
                .Where(f => f.Type != TeensyFileType.Unknown)
                .ToList();

            if (!extractedFiles.Any())
            {
                _alert.Enqueue($"No compatible files found in ZIP {fileMessageString}.");
            }

            var finalList = files
                .Where(f => !zipFiles.Any(zip => f.Name == zip.Name))
                .ToList();

            finalList.AddRange(extractedFiles);

            return finalList;
        }

        private List<FileTransferItem> ExtractD64(List<FileTransferItem> files)
        {
            if (files.Any(files => files.Type == TeensyFileType.D64))
            {
                _alert.Enqueue("D64 files detected.  Unpacking PRGs.");
            }
            var extractedPrgs = files
                .Where(f => f.Type == TeensyFileType.D64)
                .Select(f => (extraction: _d64Extractor.Extract(f), d64: f))
                .Select(f => f.extraction.ExtractedFiles
                    .Select(prgInfo => new FileTransferItem
                    (
                        sourcePath: prgInfo.FullName,
                        targetPath: f.d64.TargetPath.UnixPathCombine(f.d64.Name),
                        targetStorage: _settings.StorageType)
                    ))
                .SelectMany(f => f)
                .ToList();

            var finalList = files
                .Where(f => f.Type is not TeensyFileType.D64)
                .ToList();

            finalList.AddRange(extractedPrgs);

            return finalList;
        }

        public Task AutoStoreFiles(IEnumerable<FileTransferItem> files)
        {
            var targetPath = files.First().TargetPath;

            return Task.Run(async () =>
            {
                await _mediator.Send(new ResetCommand());
                var saveResults = await _storage.SaveFiles(files);

                if (saveResults.FailedFiles.Any())
                {
                    _alert.Enqueue($"{saveResults.FailedFiles.Count} files had an error being copied. \r\n See Logs.");
                    saveResults.FailedFiles.ForEach(d => _log.InternalError($"Error copying file: {d.SourcePath}"));
                }
                _alert.Enqueue($"{files.Count() - saveResults.FailedFiles.Count} files were saved to the TR.");

                if (_settings.AutoLaunchOnCopyEnabled) return;


                await Application.Current.Dispatcher.Invoke(async () =>
                {
                    if (targetPath == _directoryState.Value.CurrentPath)
                    {
                        await LoadDirectory(targetPath);
                    }
                });
            });
        }

        public void SetScope(StorageScope scope) => _currentScope.OnNext(scope);
        public void SetScopePath(string path) => _currentScopePath.OnNext(path);
        public StorageScope GetScope() => _currentScope.Value;
        public string GetScopePath() => _currentScopePath.Value;

        public Task PlaySubtune(int subtuneIndex)
        {
            if (IsBusy()) return Task.CompletedTask;
            return _mediator.Send(new PlaySubtuneCommand(subtuneIndex));
        }

        public Task SetSpeed(double percentage, MusicSpeedCurveTypes curveType)
        {
            if (IsBusy()) return Task.CompletedTask;
            return _mediator.Send(new SetMusicSpeedCommand(percentage, curveType));
        }
    }
}