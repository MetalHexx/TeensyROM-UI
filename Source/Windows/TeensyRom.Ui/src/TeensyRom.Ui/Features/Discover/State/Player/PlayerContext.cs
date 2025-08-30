using MediatR;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Windows;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Commands.File.LaunchFile;
using TeensyRom.Core.Commands.PlaySubtune;
using TeensyRom.Core.Common;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Storage.Tools.D64Extraction;
using TeensyRom.Core.Storage.Tools.Zip;
using TeensyRom.Ui.Controls.DirectoryTree;
using TeensyRom.Ui.Features.Common.Models;
using TeensyRom.Ui.Features.Discover.State.Directory;
using TeensyRom.Ui.Core.Progress;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Services;
using SystemDirectory = System.IO.Directory;
using TeensyRom.Core.Commands.SetMusicSpeed;
using TeensyRom.Core.Music;
using TeensyRom.Core.Commands.MuteSidVoices;
using TeensyRom.Core.Serial.Commands.Composite.StartSeek;
using TeensyRom.Core.Serial.Commands.Composite.EndSeek;
using TeensyRom.Core.Serial.Commands.Composite.FastForward;
using TeensyRom.Core.Serial.Commands.Composite.EndFastForward;
using TeensyRom.Core.Storage;
using TeensyRom.Core.Serial.Commands.ToggleMusic;
using TeensyRom.Core.Abstractions;
using TeensyRom.Core.ValueObjects;

namespace TeensyRom.Ui.Features.Discover.State.Player
{
    public class PlayerContext : IPlayerContext
    {
        public IObservable<DirectoryPath> CurrentPath => _directoryState.Select(d => d.CurrentPath);
        public IObservable<PlayerState> CurrentState => _currentState.AsObservable();
        public IObservable<int> CurrentPage => _directoryState.Select(d => d.CurrentPage);
        public IObservable<int> TotalPages => _directoryState.Select(d => d.TotalPages);
        public IObservable<bool> PagingEnabled => _directoryState.Select(d => d.PagingEnabled);
        public IObservable<DirectoryNodeViewModel?> DirectoryTree => _tree.DirectoryTree.AsObservable();
        public IObservable<ObservableCollection<StorageItem>> DirectoryContent => _directoryState.Select(d => d.DirectoryContent);
        public IObservable<LaunchedFileResult> LaunchedFile => _launchedFile.AsObservable();        
        public IObservable<LaunchableItem> SelectedFile => _selectedFile.AsObservable();
        public IObservable<PlayState> PlayingState => _playingState.AsObservable();
        public IObservable<StorageScope> CurrentScope => _currentScope.AsObservable();
        public IObservable<DirectoryPath> CurrentScopePath => _currentScopePath.AsObservable();

        private DirectoryPath _currentPath = new DirectoryPath(string.Empty);
        private string _previousSearch = string.Empty;
        private TimeSpan _currentTime = TimeSpan.Zero;        

        protected PlayerState? _previousState;
        protected readonly BehaviorSubject<PlayerState> _currentState;
        protected readonly BehaviorSubject<LaunchedFileResult> _launchedFile = new(null!);        
        protected readonly BehaviorSubject<LaunchableItem> _selectedFile = new(null!);
        protected readonly BehaviorSubject<PlayState> _playingState = new(PlayState.Stopped);
        protected readonly BehaviorSubject<StorageScope> _currentScope = new(StorageScope.Storage);
        protected readonly BehaviorSubject<DirectoryPath> _currentScopePath = new(new DirectoryPath(StorageHelper.Remote_Path_Root));
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
        protected bool _isInitLoadedOnly = false;

        public PlayerContext(IMediator mediator, ICachedStorageService storage, ISettingsService settingsService, ILaunchHistory launchHistory, ID64Extractor d64Extractor, IZipExtractor zipExtractor, ISnackbarService alert, ISerialStateContext serialContext, INavigationService nav, IDiscoveryTreeState tree, IProgressTimer timer, ILoggingService log, IFileTransferService fileTransfer)
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

                    var fileToAutoLaunch = files.OfType<LaunchableItem>().FirstOrDefault();

                    if (fileToAutoLaunch is not null && _settings.AutoLaunchOnCopyEnabled)
                    {
                        await LoadDirectory(fileToAutoLaunch.Path.Directory, fileToAutoLaunch.Path);
                        await PlayFile(fileToAutoLaunch);
                    }
                });

            storage.FilesChanged
                .SubscribeOn(RxApp.MainThreadScheduler)
                .Subscribe(async files =>
                {
                    await UpdateDirectoryTree(files.ToList());
                });

            storage.FilesDeleted
                .SubscribeOn(RxApp.MainThreadScheduler)
                .SelectMany(m => m)
                .Subscribe(async file => await OnDelete(file));

            timer.CurrentTime.Subscribe(time => _currentTime = time);
        }

        private void SubscribeToStateObservables()
        {
            var settingsObservable = _settingsService.Settings
                .Where(settings => settings is not null)
                .Do(settings => _settings = settings);

            settingsObservable
                .Take(1)
                .Subscribe(s => _currentFilter = _settings.GetStartupFilter());

            var navAndStorageObservable = settingsObservable
                .Where(s => s.LastCart is not null)
                .CombineLatest(
                    _serial.CurrentState,
                    _nav.SelectedNavigationView,
                    _storage.StorageReady,
                    (settings, serial, navView, storageReady) => (settings, serial, navView))
                .Where(state =>
                    state.navView?.Type == NavigationLocation.Discover &&
                    state.serial is SerialConnectedState)
                .Select(state => state.settings)
                .DistinctUntilChanged(settings => $"{settings.StorageType}|{settings.LastCart?.DeviceHash}");
                

            _settingsSubscription = navAndStorageObservable
                .Subscribe(async _ => 
                {
                    _tree.ResetDirectoryTree(new DirectoryPath(StorageHelper.Remote_Path_Root));
                    await LoadDirectory(new DirectoryPath(StorageHelper.Remote_Path_Root));
                });

            var initializeObservable = navAndStorageObservable
                .Where(s => s.FirstTimeSetup == false)
                .Take(1);
                

            initializeObservable
                .Where(settings => settings.StartupLaunchEnabled)                
                .Subscribe(async _ => await HandleStartupWithLaunch());

            initializeObservable
                .Where(s => s.StartupLaunchEnabled is false)                
                .Subscribe(async s => await HandleStartupWithoutLaunch());
        }

        private async Task HandleStartupWithoutLaunch()
        {
            if (_settings.LastCart!.LastFile is not null)
            {
                await LoadDirectory(_settings.LastCart.LastFile.Path.Directory, _settings.LastCart.LastFile.Path);
                _isInitLoadedOnly = true;
                await LoadFileWithoutLaunch(_settings.LastCart.LastFile);
                return;
            }
            await LoadDirectory(new DirectoryPath(StorageHelper.Remote_Path_Root));
        }

        private async Task HandleStartupWithLaunch()
        {
            if (_settings.StartupLaunchRandom || _settings.LastCart!.LastFile is null) 
            {
                await PlayRandom();
                return;
            }

            await LoadDirectory(_settings.LastCart.LastFile.Path.Directory, _settings.LastCart.LastFile.Path);
            await PlayFile(_settings.LastCart.LastFile);
        }

        private async Task UpdateDirectoryTree(List<FileItem> files)
        {
            var uniquePaths = files
                .Select(f => f.Path.Directory.Value)
                .Distinct();

            if (Application.Current?.Dispatcher is null) 
            {
                await UpdateDirectoryTree(uniquePaths);
                return;
            }

            await Application.Current.Dispatcher.Invoke(async () =>
            {
                await UpdateDirectoryTree(uniquePaths);
            });
        }

        private async Task UpdateDirectoryTree(IEnumerable<string> uniquePaths)
        {
            foreach (var path in uniquePaths)
            {
                List<string> directoryParts = ["/"];

                directoryParts.AddRange(path.ToPathArray());

                var currentDir = new DirectoryPath(string.Empty);

                List<DirectoryItem> dirsToAdd = [];

                foreach (var directory in directoryParts.Take(2))
                {
                    currentDir = new DirectoryPath(currentDir.ToString().UnixPathCombine(directory));
                    var cacheItem = await _storage.GetDirectory(currentDir);

                    if (cacheItem is not null)
                    {
                        _tree.Insert(cacheItem.Directories);
                    }
                }
            }
            if (_currentState.Value is not SearchState)
            {
                await LoadDirectory(_directoryState.Value.CurrentPath);
            }
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
        public Task LoadDirectory(DirectoryPath path)
        {
            return LoadDirectory(path, null);
        }
        public async Task LoadDirectory(DirectoryPath path, FilePath? filePathToSelect = null)
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
                if (_currentPath.IsEmpty) return;

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
            if (_isInitLoadedOnly) 
            {
                _isInitLoadedOnly = false;
                await PlayFile(_launchedFile.Value.File);
                return;
            }
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

        private async Task LoadFileWithoutLaunch(LaunchableItem file)
        {
            if (file is null) return;

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
            await LoadDirectory(file.Path.Directory, file.Path);
            _launchedFile.OnNext(GetLaunchResult(file));
            _selectedFile.OnNext(file);

            if (file is SongItem)
            {
                _playingState.OnNext(PlayState.Paused);
                return;
            }
            _playingState.OnNext(PlayState.Stopped);
        }

        public virtual async Task PlayFile(LaunchableItem file)
        {
            if (file is null) return;

            //if (!file.IsCompatible)
            //{
            //    await HandleBadFile(file);
            //    return;
            //}
            if (await IsBusy()) return;
            SaveLastPlayedFile(file);
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

        private void SaveLastPlayedFile(LaunchableItem file)
        {
            var settings = _settingsService.GetSettings();

            var cart = settings.KnownCarts.FirstOrDefault(c => c.DeviceHash == settings.LastCart?.DeviceHash);

            if (cart is not null)
            {
                var newCart = cart with { LastFile = file };
                var cartIndex = settings.KnownCarts.IndexOf(cart);
                settings.KnownCarts.Remove(cart);
                settings.KnownCarts.Add(newCart);
                _settingsService.SaveSettings(settings);
            }
        }

        private LaunchedFileResult GetLaunchResult(LaunchableItem file) 
        {
            var isRandom = _currentState.Value is ShuffleState && _launchHistory.CurrentIsNew && file.Path != _launchedFile.Value?.File.Path;
            return new LaunchedFileResult(file, isRandom);
        }
        private async Task<bool> IsBusy()
        {
            var serialState = await _serial.CurrentState.FirstOrDefaultAsync();

            return serialState is SerialBusyState;
        }

        private void LaunchFileAsync(LaunchableItem file)
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

        public Task HandleBadFile(LaunchableItem file)
        {
            _alert.Enqueue($"{file.Name} is currently unsupported (see logs).  Skipping to the next file.");

            _launchedFile.OnNext(GetLaunchResult(file)); //play next will use this to determine the next file            

            if (file.IsCompatible is true) _storage.MarkIncompatible(file);

            _launchHistory.Remove(file);

            return PlayNext();
        }

        private async Task OnDelete(FileItem file) 
        {
            await Task.Delay(200);            

            await LoadDirectory(_directoryState.Value.CurrentPath);
        }

        public async Task DeleteFile(FileItem file)
        {
            if (_directoryState.Value is null) return;

            await _storage.DeleteFile(file, _settings.StorageType);
        }
        public async Task PlayNext()
        {
            if (await IsBusy()) return;

            var file = await _currentState.Value.GetNext(_launchedFile.Value?.File, _currentFilter.Type, _directoryState.Value);

            if (file is null) 
            {
                _alert.Enqueue("No file was found matching you selected filter.");
                return;
            }
            await PlayFile(file);
        }
        public async Task PlayPrevious()
        {
            if (await IsBusy()) return;

            if (_launchedFile.Value.File is SongItem && _currentTime >= TimeSpan.FromSeconds(3))
            {
                await PlayFile(_launchedFile.Value!.File);
                return; 
            }
            if (_launchedFile.Value.File is LaunchableItem)
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

        public async Task<LaunchableItem?> PlayRandom()
        {
            if (await IsBusy()) return _launchedFile.Value?.File;

            var success = TryTransitionTo(typeof(ShuffleState));

            if (!success) return _launchedFile.Value?.File;

            _launchHistory.ClearForwardHistory();
            await PlayNext();
            return _launchedFile.Value?.File;
        }

        public void UpdateHistory(LaunchableItem fileToLoad)
        {
            var isNotConsecutiveDuplicate = _launchedFile.Value?.File is null
                || !fileToLoad.Path.Equals(_launchedFile.Value.File.Path);

            if (isNotConsecutiveDuplicate) _launchHistory.Add(fileToLoad);
        }
        public Unit SearchFiles(string searchText)
        {
            _previousSearch = searchText;

            var success = TryTransitionTo(typeof(SearchState));

            if (!success) return Unit.Default;

            var searchResult = _storage.Search(searchText, GetFileTypes())
                .Cast<StorageItem>()
                .ToList();

            if (searchResult is null) return Unit.Default;

            _directoryState.Value.ClearSelection();
            _directoryState.Value.LoadDirectory(searchResult);
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
            _tree.ResetDirectoryTree(new DirectoryPath(StorageHelper.Remote_Path_Root));
            await LoadDirectory(new DirectoryPath(StorageHelper.Remote_Path_Root));
        }
        public Unit SelectFile(LaunchableItem file)
        {
            file.IsSelected = true;

            var shouldUpdateCurrent = _selectedFile.Value is not null
                && !file.Path.Equals(_selectedFile.Value.Path);

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

        private void SwitchFilterByFileType(LaunchableItem file) 
        {
            _currentFilter = file switch
            {
                SongItem songItem => _settings.FileFilters.First(f => f.Type == TeensyFilterType.Music),
                GameItem gameItem => _settings.FileFilters.First(f => f.Type == TeensyFilterType.Games),
                ImageItem imageItem => _settings.FileFilters.First(f => f.Type == TeensyFilterType.Images),
                _ => _settings.FileFilters.First(f => f.Type == TeensyFilterType.All)

            };
        }

        public async Task SwitchFilterAndLaunch(TeensyFilter filter)
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

        public void SetScope(StorageScope scope) => _currentScope.OnNext(scope);
        public void SetScopePath(DirectoryPath path) 
        {
            if (_currentScopePath.Value.Equals(path)) 
            {
                _currentScopePath.OnNext(new DirectoryPath(StorageHelper.Remote_Path_Root));
                return;
            }
            _currentScopePath.OnNext(path);
        }
        public StorageScope GetScope() => _currentScope.Value;
        public DirectoryPath GetScopePath() => _currentScopePath.Value;

        public async Task<PlaySubtuneResult?> PlaySubtune(int subtuneIndex)
        {
            if (await IsBusy()) return null;

            return await _mediator.Send(new PlaySubtuneCommand(subtuneIndex));
        }

        public async Task Mute(VoiceState voice1, VoiceState voice2, VoiceState voice3)
        {
            if (await IsBusy()) return;

            await _mediator.Send(new MuteSidVoicesCommand(voice1, voice2, voice3));
        }

        public async Task SetSpeed(double percentage, MusicSpeedCurveTypes curveType)
        {
            if (await IsBusy()) return;

            await _mediator.Send(new SetMusicSpeedCommand(percentage, curveType));
        }

        public async Task RestartSong() 
        {
            var launchedFile = _launchedFile.Value.File;

            if (launchedFile is null) return;

            await _mediator.Send(new LaunchFileCommand(_settings.StorageType, launchedFile));
            _playingState.OnNext(PlayState.Playing);
        }

        public async Task<PlaySubtuneResult?> RestartSubtune(int subtuneIndex)
        {
            var result = await _mediator.Send(new PlaySubtuneCommand(subtuneIndex));

            if (result.IsSuccess is true) 
            {
                _playingState.OnNext(PlayState.Playing);
            }
            return result;
        }

        public async Task StartSeek(int subtuneIndex, bool togglePlay, bool muteVoices, double speed, SeekDirection direction)
        {
            if (await IsBusy()) return;

            var result = await _mediator.Send(new StartSeekCommand 
            {
                SubtuneIndex = subtuneIndex,
                ShouldTogglePlay = togglePlay,                
                ShouldMuteVoices = muteVoices,
                SeekSpeed = speed,
                Direction = direction
            });

            if (togglePlay) 
            {
                _playingState.OnNext(_playingState.Value == PlayState.Paused
                    ? PlayState.Playing
                    : PlayState.Paused);
            }
        }

        public async Task EndSeek(bool enableVoices, double originalSpeed, MusicSpeedCurveTypes speedCurve)
        {
            if (await IsBusy()) return;

            var result = await _mediator.Send(new EndSeekCommand
            {
                ShouldEnableVoices = enableVoices,
                SeekSpeed = originalSpeed,
                SpeedCurve = speedCurve
            });
        }

        public async Task FastForward(bool togglePlay, bool muteVoices, double speed)
        {
            if (await IsBusy()) return;

            var result = await _mediator.Send(new FastForwardCommand
            {
                ShouldTogglePlay = togglePlay,
                ShouldMuteVoices = muteVoices,
                Speed = speed,
            });

            if (togglePlay)
            {
                _playingState.OnNext(_playingState.Value == PlayState.Paused
                    ? PlayState.Playing
                    : PlayState.Paused);
            }
        }

        public async Task EndFastForward(bool enableVoices, double originalSpeed, MusicSpeedCurveTypes speedCurve)
        {
            if (await IsBusy()) return;

            var result = await _mediator.Send(new EndFastForwardCommand
            {
                ShouldEnableVoices = enableVoices,
                OriginalSpeed = originalSpeed,
                SpeedCurve = speedCurve
            });
        }
    }
}