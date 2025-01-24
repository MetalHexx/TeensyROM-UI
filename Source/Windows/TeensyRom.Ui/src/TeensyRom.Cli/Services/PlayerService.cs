using Spectre.Console;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using TeensyRom.Cli.Helpers;
using TeensyRom.Core.Storage.Services;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Player;
using TeensyRom.Player;

namespace TeensyRom.Cli.Services
{
    internal class PlayerService : IPlayerService
    {
        public IObservable<ILaunchableItem> FileLaunched => _fileLaunched.AsObservable();
        private Subject<ILaunchableItem> _fileLaunched = new();
        private PlayerState _state;

        private readonly ICachedStorageService _storage;
        private readonly ISettingsService _settingsService;
        private readonly IAlertService _alert;
        private readonly IPlayer _player;

        public PlayerService(ICachedStorageService storage, ISettingsService settingsService, ISerialStateContext serial, IAlertService alert, IPlayer player)
        {
            _storage = storage;
            _settingsService = settingsService;
            _alert = alert;
            _player = player;
            var settings = settingsService.GetSettings();

            _state = new() 
            {
                StorageType = settings.StorageType,
                FilterType = settings.StartupFilter,                
            };
            serial.CurrentState
                .Where(state => state is SerialConnectionLostState && _state.PlayState is Player.PlayerState.Playing)
                .Subscribe(_ => StopStream()); 

            player.CurrentItem.Subscribe(OnCurrentItem);
            
            player.BadFile
                .Where(item => item is not null)
                .Subscribe(OnBadFile);

            player.PlayerState
                .Subscribe(state => _state = _state with 
                { 
                    PlayState = state 
                });
        }

        private async void OnBadFile(ILaunchableItem item)
        {
            _storage.MarkIncompatible(item);            
            AlertBadFile(item);
            await PlayNext();
            _player.RemoveItem(item);
        }

        private void OnCurrentItem(ILaunchableItem? item)
        {
            if (item is null) return;

            _state.CurrentItem = item;
            _fileLaunched.OnNext(item);
            _alert.Publish(RadHelper.ClearHack);
        }

        public async Task Launch(ILaunchableItem item)
        {
            await _player.PlayItem(item);
        }

        public async Task LaunchItem(string path)
        {   
            await _player.PlayItem(path);
        }

        private List<ILaunchableItem> GetUnfilteredLaunchables(StorageCacheItem directory)
        {
            return directory.Files
                .OfType<ILaunchableItem>()
                .Where(file => file.IsCompatible)
                .ToList();
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
                throw new Exception("You must set random mode first.");
            }
            await _player.PlayNext();
        }

        public async Task PlayPrevious()
        {
            await _player.PlayPrevious();            
        }

        public async Task PlayNext()
        {
            if (_state.PlayMode is PlayMode.Random)
            {
                await PlayRandom();
                return;
            }
            await _player.PlayNext();
            return;
        }

        public void StopStream()
        {
            _player.Stop();
        }

        public void PauseStream() 
        {
            _player.Pause();
        }
        public void ResumeStream() 
        {
            _player.ResumeItem();
        }

        public PlayerState GetState() => _state with { };

        public List<ILaunchableItem> SetSearchMode(string query)
        {
            _state = _state with
            {
                PlayMode = PlayMode.Search,
                SearchQuery = query
            };

            var searchResults = _storage.Search(query);

            if (!searchResults.Any())
            {
                _alert.PublishError("No files found for the search query.");
                return [];
            }
            _player.SetPlaylist(searchResults.ToList());
            var filterFileTypes = GetFilterFileTypes();
            
            return searchResults
                .Where(f => filterFileTypes.Any(type => type == f.FileType))
                .ToList();
        }

        public async Task<List<ILaunchableItem>> SetDirectoryMode(string directoryPath)
        {
            _state = _state with
            {
                PlayMode = PlayMode.CurrentDirectory,
                FilterType = TeensyFilterType.All,
                SearchQuery = null
            };
            _player.SetFilter(TeensyFilterType.All);
            _alert.Publish("Switching filter to \"All\"");

            var directory = await _storage.GetDirectory(directoryPath);

            if (directory is null)
            {
                _alert.PublishError("Directory not found.");
                return [];
            }
            var launchableFiles = GetUnfilteredLaunchables(directory);

            if (launchableFiles.Count == 0)
            {
                _alert.PublishError("No launchable files found in the directory.");
                return launchableFiles;
            }
            _player.SetPlaylist(launchableFiles);            
            
            return launchableFiles;
        }

        public void SetRandomMode()
        {
            if (_state.PlayMode is not PlayMode.Random)
            {
                LoadRandomFiles();
            }
            _state = _state with            
            {
                PlayMode = PlayMode.Random,
                SearchQuery = null
            };
        }

        private void LoadRandomFiles()
        {
            var playlist = _storage.GetRandomFiles(_state.Scope, _state.ScopePath);

            if (playlist.Count == 0)
            {
                _alert.PublishError($"No files were found on the {_state.StorageType} in {_state.ScopePath}.");
                return;
            }
            _player.SetPlaylist(playlist);
        }

        public void SetFilter(TeensyFilterType filterType) 
        {
            _state = _state with { FilterType = filterType };
            _player.SetFilter(_state.FilterType);
        }
        public void SetDirectoryScope(string path) 
        {
            _state = _state with { ScopePath = path };

            if (_state.PlayMode is PlayMode.Random) 
            {
                LoadRandomFiles();
            };
        }
        public void SetStreamTime(TimeSpan? timespan)
        {
            if (!timespan.HasValue) return;

            _player.SetPlayTimer(timespan.Value);
            _state = _state with { PlayTimer = timespan };
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
            return StorageHelper.GetFileTypes(_state.FilterType);
        }

        public void TogglePlay()
        {
            if(_state.PlayState is Player.PlayerState.Playing)
            {
                if (_state.CurrentItem is SongItem)
                {
                    _player.Pause();
                    _alert.Publish($"{_state.CurrentItem?.Name} has been paused.");
                }
                else 
                {
                    _player.Stop();
                    _alert.Publish($"{_state.CurrentItem?.Name} has been stopped.");
                }
                return;
            }
            _player.ResumeItem();
            _alert.Publish($"{_state.CurrentItem?.Name} has resumed.");
        }
    }
}