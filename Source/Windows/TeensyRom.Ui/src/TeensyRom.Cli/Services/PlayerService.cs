using MediatR;
using Spectre.Console;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using TeensyRom.Cli.Helpers;
using TeensyRom.Cli.Core.Commands;
using TeensyRom.Cli.Core.Commands.File.LaunchFile;
using TeensyRom.Cli.Core.Player;
using TeensyRom.Cli.Core.Progress;
using TeensyRom.Cli.Core.Serial.State;
using TeensyRom.Cli.Core.Settings;
using TeensyRom.Cli.Core.Storage.Services;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Common;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Cli.Services
{
    internal class PlayerService : IPlayerService
    {
        public IObservable<ILaunchableItem> FileLaunched => _fileLaunched.AsObservable();
        private Subject<ILaunchableItem> _fileLaunched = new();
        private PlayerState _state;


        private IDisposable? _timerSubscription;
        private readonly IMediator _mediator;
        private readonly ICachedStorageService _storage;
        private readonly IProgressTimer _timer;
        private readonly ISettingsService _settingsService;
        private readonly ILaunchHistory _randomHistory;
        private readonly IAlertService _alert;

        public PlayerService(IMediator mediator, ICachedStorageService storage, IProgressTimer progressTimer, ISettingsService settingsService, ISerialStateContext serial, ILaunchHistory history, IAlertService alert)
        {
            _mediator = mediator;
            _storage = storage;
            _timer = progressTimer;
            _settingsService = settingsService;
            _randomHistory = history;
            _alert = alert;
            var settings = settingsService.GetSettings();

            _state = new() 
            {
                StorageType = settings.StorageType,
                FilterType = settings.StartupFilter,                
            };

            serial.CurrentState
                .Where(state => state is SerialConnectionLostState && _state.PlayState is PlayState.Playing)
                .Subscribe(_ => StopStream());

            SetupTimerSubscription();
        }

        public async Task LaunchFromDirectory(string path)
        {
            var directory = await _storage.GetDirectory(path.GetUnixParentPath());

            if (directory is null)
            {
                _alert.PublishError("Directory not found.");
                return;
            }
            var fileItem = directory.Files.FirstOrDefault(f => f.Path.Contains(path));

            if (fileItem is not ILaunchableItem launchItem)
            {
                _alert.PublishError("File is not launchable.");
                return;
            }
            var launchSuccessful = await LaunchItem(launchItem);

            if (!launchSuccessful) 
            {
                await PlayNext();
            }
        }

        public async Task<bool> LaunchItem(ILaunchableItem item)

        {
            _state = _state with
            {
                CurrentItem = item,
                PlayState = PlayState.Playing
            };
            if (!item.IsCompatible) 
            {
                AlertBadFile(item);
                return false;
            }
            var result = await _mediator.Send(new LaunchFileCommand(_state.StorageType, item));

            if (result.IsSuccess)
            {
                _fileLaunched.OnNext(item);
                _alert.Publish(RadHelper.ClearHack);
                MaybeStartStream(item);
                return true;
            }          
            if (result.LaunchResult is LaunchFileResultType.SidError or LaunchFileResultType.ProgramError)
            {
                _storage.MarkIncompatible(item);
                _randomHistory.Remove(item);
                AlertBadFile(item);
                return false;
            }
            _alert.PublishError($"Failed to launch {item.Name}.");
            return false;
        }

        private void AlertBadFile(ILaunchableItem item) 
        {
            AnsiConsole.WriteLine();
            _alert.PublishError($"{item.Name} is currently unsupported (see logs).  Skipping file.");
        }

        public async Task PlayRandom()
        {
            if (_state.PlayMode is not PlayMode.Random)
            {
                _randomHistory.Clear();
            }
            _state.PlayMode = PlayMode.Random;

            var randomItem = _storage.GetRandomFile(_state.Scope, _state.ScopePath, GetFilterFileTypes());

            if (randomItem is null)
            {
                _alert.PublishError($"No files for the filter \"{_state.FilterType}\" were found on the {_state.StorageType} in {_state.ScopePath}.");
                return;
            }
            var launchSuccessful = await LaunchItem(randomItem);

            if (launchSuccessful) 
            {
                _randomHistory.Add(randomItem);
                return;
            }
            await PlayNext();
        }

        public async Task PlayPrevious()
        {
            ILaunchableItem? fileToPlay = _state.PlayMode switch
            {
                PlayMode.Random => _randomHistory.GetPrevious(GetFilterFileTypes()),
                PlayMode.Search => GetPreviousSearchItem(),
                PlayMode.CurrentDirectory => await GetPreviousDirectoryItem(),
                _ => _state.CurrentItem
            };
            if(fileToPlay is null)
            {
                if (_state.CurrentItem is null) 
                {
                    return;
                }
                fileToPlay = _state.CurrentItem;
            }
            var launchSuccessful = await LaunchItem(fileToPlay);

            if (!launchSuccessful) 
            {
                await PlayPrevious();
            }
        }

        public ILaunchableItem? GetPreviousSearchItem()
        {
            var list = _storage.Search(_state.SearchQuery!, []).ToList();
            return GetPreviousFromList(list);
        }

        public async Task<ILaunchableItem?> GetPreviousDirectoryItem()
        {
            var currentPath = _state.CurrentItem!.Path.GetUnixParentPath();
            var currentDirectory = await _storage.GetDirectory(currentPath);

            if (currentDirectory is null)
            {
                _alert.PublishError($"Couldn't find directory {currentPath}.");
                return null;
            }
            var files = currentDirectory.Files.OfType<ILaunchableItem>().ToList();
            return GetPreviousFromList(files);
        }

        private ILaunchableItem? GetPreviousFromList(List<ILaunchableItem> list)
        {
            var unfilteredFiles = list;

            if (unfilteredFiles.Count == 0)
            {
                _alert.PublishError("Something went wrong.  I couldn't find any files in the target location.");
                return null;
            }
            var currentFile = unfilteredFiles.ToList().FirstOrDefault(f => f.Id == _state.CurrentItem?.Id);

            if (currentFile is null)
            {
                _alert.PublishError("Something went wrong.  I couldn't find the current file in the target location.");
                return null;
            }

            var filteredFiles = unfilteredFiles
                .Where(f => GetFilterFileTypes()
                    .Any(t => f.FileType == t))
                .ToList();

            if (filteredFiles.Count() == 0)
            {
                _alert.PublishError("There were no files matching your filter in the target location");
                return null;
            }

            var currentFileUnfilteredIndex = unfilteredFiles.IndexOf(currentFile);

            var currentFileComesAfterLastItemInFilteredList = unfilteredFiles.IndexOf(filteredFiles.Last()) < currentFileUnfilteredIndex;

            if (currentFileComesAfterLastItemInFilteredList)
            {
                return filteredFiles.Last();
            }
            var filteredIndex = filteredFiles.IndexOf(currentFile);

            if (filteredIndex != -1)
            {
                var index = filteredIndex == 0
                    ? filteredFiles.Count - 1
                    : filteredIndex - 1;

                return filteredFiles[index];
            }

            ILaunchableItem? candidate = null;

            for (int x = 0; x < filteredFiles.Count; x++)
            {
                var f = filteredFiles[x];

                var fIndex = unfilteredFiles.IndexOf(f);

                if (fIndex < currentFileUnfilteredIndex)
                {
                    candidate = f;
                    continue;
                }
                else if (fIndex > currentFileUnfilteredIndex)
                {
                    break;
                }
            }
            if (candidate is null)
            {
                return filteredFiles.First();
            }
            return candidate;
        }

        public async Task PlayNext()
        {
            ILaunchableItem? fileToPlay = null;

            switch (_state.PlayMode) 
            {
                case PlayMode.Random:
                    fileToPlay = _randomHistory.GetNext(GetFilterFileTypes());

                    if (fileToPlay is null)
                    {
                        await PlayRandom();
                        return;
                    }
                    break;

                case PlayMode.Search:
                    fileToPlay = GetNextSearchItem();
                    break;

                case PlayMode.CurrentDirectory:
                    fileToPlay = await GetNextDirectoryItem();
                    break;
            }

            if (fileToPlay is null)
            {
                fileToPlay = _state.CurrentItem;
            }
            var launchSuccessful = await LaunchItem(fileToPlay!);

            if (!launchSuccessful) await PlayNext();
        }

        public ILaunchableItem? GetNextSearchItem()
        {
            if (_state.SearchQuery is null) return null;

            var list = _storage.Search(_state.SearchQuery, []).ToList();

            return GetNextListItem(list);
        }

        public async Task<ILaunchableItem?> GetNextDirectoryItem()
        {
            var currentPath = _state.CurrentItem!.Path.GetUnixParentPath();
            var currentDirectory = await _storage.GetDirectory(currentPath);

            if (currentDirectory is null) return null;

            var compatibleFiles = currentDirectory.Files.Any(f => f.IsCompatible);

            if (!compatibleFiles) return null;        
            
            var list = currentDirectory.Files.OfType<ILaunchableItem>().ToList();
            return GetNextListItem(list);
        }

        public ILaunchableItem? GetNextListItem(List<ILaunchableItem> list)
        {
            var unfilteredFiles = list;

            if (unfilteredFiles.Count == 0)
            {
                RadHelper.WriteError("Something went wrong.  I coudln't find any files in the target location.");
                return null;
            }

            var currentFile = unfilteredFiles.ToList().FirstOrDefault(f => f.Id == _state.CurrentItem?.Id);

            if (currentFile is null)
            {
                RadHelper.WriteError("Something went wrong.  I coudln't find the current file in the target location.");
                return null;
            }

            var unfilteredIndex = unfilteredFiles.IndexOf(currentFile);

            var filteredFiles = unfilteredFiles
                .Where(f => GetFilterFileTypes()
                    .Any(t => f.FileType == t))
                .ToList();

            if (filteredFiles.Count() == 0)
            {
                RadHelper.WriteError("There were no files matching your filter in the target location");
                return null;
            }
            if (unfilteredIndex > filteredFiles.Count - 1)
            {
                return filteredFiles.First();
            }
            var filteredIndex = filteredFiles.IndexOf(currentFile);

            if (filteredIndex != -1)
            {
                var index = filteredIndex < filteredFiles.Count - 1
                ? filteredIndex + 1
                : 0;

                return filteredFiles[index];
            }

            ILaunchableItem? candidate = null;

            for (int x = 0; x < filteredFiles.Count; x++)
            {
                var f = filteredFiles[x];

                var fIndex = unfilteredFiles.IndexOf(f);

                if (fIndex > unfilteredIndex)
                {
                    candidate = f;
                    continue;
                }
                else if (fIndex < unfilteredIndex)
                {
                    break;
                }
            }
            if (candidate is null)
            {
                return filteredFiles.First();
            }
            return candidate;
        }

        private void MaybeStartStream(ILaunchableItem fileItem)
        {
            if (fileItem is SongItem songItem && _state.SidTimer is SidTimer.SongLength)
            {
                StartStream(songItem.PlayLength);
                return;
            }
            if (_state.PlayTimer is not null)
            {
                StartStream(_state.PlayTimer.Value);
            }
        }

        private void StartStream(TimeSpan length)
        {
            _state.PlayState = PlayState.Playing;
            SetupTimerSubscription();
            _timer.StartNewTimer(length);
        }

        private void SetupTimerSubscription()
        {
            _timerSubscription?.Dispose();

            _timerSubscription = _timer.TimerComplete.Subscribe(async _ =>
            {
                await PlayNext();
            });
        }

        public void StopStream()
        {
            if (_timerSubscription is not null)
            {
                _alert.Publish("Stopping Stream");
            }
            _state.PlayState = PlayState.Stopped;
            _timerSubscription?.Dispose();
            _timerSubscription = null;
        }

        public void PauseStream() 
        {
            _state.PlayState = PlayState.Paused;
            _timer.PauseTimer();
        }
        public void ResumeStream() 
        {
            _state.PlayState = PlayState.Playing;
            _timer.ResumeTimer();
        }

        public PlayerState GetState() => _state with { };

        public void SetSearchMode(string query)
        {
            _state = _state with
            {
                PlayMode = PlayMode.Search,
                SearchQuery = query
            };
        }

        public void SetDirectoryMode(string directoryPath)
        {
            _state = _state with 
            {
                PlayMode = PlayMode.CurrentDirectory,
                FilterType = TeensyFilterType.All,
                ScopePath = directoryPath,
                SearchQuery = null
            };
            _alert.Publish("Switching filter to \"All\"");
        }

        public void SetRandomMode(string scopePath)
        {
            if (_state.PlayMode is not PlayMode.Random)
            {
                _randomHistory.Clear();
            }
            _state = _state with            
            {
                PlayMode = PlayMode.Random,
                ScopePath = scopePath,
                SearchQuery = null
            };
        }

        public void SetFilter(TeensyFilterType filterType) => _state = _state with { FilterType = filterType };
        public void SetDirectoryScope(string path) => _state = _state with { ScopePath = path };
        public void SetStreamTime(TimeSpan? timespan)
        {
            _state = _state with { PlayTimer = timespan };

            if (_state.CurrentItem is SongItem && _state.SidTimer is SidTimer.SongLength)
            {
                return;
            }

            if (_state.PlayTimer is not null)
            {
                StopStream();
                StartStream(_state.PlayTimer.Value);
            }
        }
        public void SetSidTimer(SidTimer value) => _state = _state with { SidTimer = value };

        public void SetStorage(TeensyStorageType storageType)
        {
            _state = _state with { StorageType = storageType };
            _storage.SwitchStorage(storageType);
        }

        private TeensyFileType[] GetFilterFileTypes()
        {
            var trSettings = _settingsService.GetSettings();
            return trSettings.GetFileTypes(_state.FilterType);
        }

        public void TogglePlay()
        {
            if(_state.PlayState is PlayState.Playing)
            {   
                PauseStream();
                
                if(_state.CurrentItem is SongItem) 
                {
                    _mediator.Send(new ToggleMusicCommand());
                    _alert.Publish($"{_state.CurrentItem.Name} has been paused.");
                    return;
                }
                _mediator.Send(new ResetCommand());
                _alert.Publish($"{_state.CurrentItem?.Name} has been stopped.");
                return;
            }
            if(_state.CurrentItem is null)
            {
                _alert.PublishError("Hit back and try starting a new stream.");
                return;
            }
            ResumeStream();

            _state = _state with { PlayState = PlayState.Playing };

            if (_state.CurrentItem is SongItem)
            {
                _mediator.Send(new ToggleMusicCommand());
            }
            else 
            {
                _mediator.Send(new LaunchFileCommand(_state.StorageType, _state.CurrentItem));
            }
            _alert.Publish($"{_state.CurrentItem.Name} has been resumed.");
        }
    }
}