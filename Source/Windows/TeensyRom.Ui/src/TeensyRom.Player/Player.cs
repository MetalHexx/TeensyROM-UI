using MediatR;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Runtime.CompilerServices;
using TeensyRom.Core.Commands.LaunchFile;
using TeensyRom.Core.Commands.Reset;
using TeensyRom.Core.Commands.ToggleMusic;
using TeensyRom.Core.Player;
using TeensyRom.Core.Progress;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Player
{
    public class Player(IProgressTimer timer, IMediator mediator, ISettingsService settingsService) : IPlayer
    {
        public IObservable<ILaunchableItem?> CurrentItem => _currentItem.AsObservable();
        private BehaviorSubject<ILaunchableItem?> _currentItem = new(null);

        public IObservable<PlayerState> PlayerState => _playerState.AsObservable();
        private BehaviorSubject<PlayerState> _playerState = new(TeensyRom.Player.PlayerState.Stopped);

        public IObservable<ILaunchableItem> BadFile => _badFile.AsObservable();
        private Subject<ILaunchableItem> _badFile = new();

        public IObservable<TimeSpan> CurrentTime => timer.CurrentTime;

        private IDisposable? _timerSubscription;

        private readonly List<ILaunchableItem> _items = [];
        private TeensyFilterType _filter = TeensyFilterType.All;
        private TimeSpan? _playTimer = null;
        private bool _songTimeOverride = false;
        private ILaunchableItem? _lastSong = null;
        private ILaunchableItem? _lastGame = null;
        private ILaunchableItem? _lastImage = null;

        public async Task PlayNext()
        {
            if (_currentItem.Value is null) 
            {
                await PlayItem(GetFilteredItems().First());
                return;
            }
            await PlayNextOrPrevious(true);
        }

        public async Task PlayPrevious()
        {
            if (_currentItem.Value is null)
            {
                await PlayItem(GetFilteredItems().Last());
                return;
            }
            await PlayNextOrPrevious(false);
        }

        private List<ILaunchableItem> GetFilteredItems() => _items
            .Where(i => StorageHelper.GetFileTypes(_filter).Contains(i.FileType))
            .GroupBy(file => file.Id)
            .Select(group => group.First())
            .ToList();

        private async Task PlayNextOrPrevious(bool isNext)
        {
            var unfilteredFiles = _items;

            if (unfilteredFiles.Count == 0) return;

            var currentFile = unfilteredFiles.FirstOrDefault(f => f.Id == _currentItem.Value?.Id);

            if (currentFile is null) return;

            var unfilteredIndex = unfilteredFiles.IndexOf(currentFile);

            var filteredFiles = GetFilteredItems();

            if (filteredFiles.Count == 0) return;

            var filteredIndex = filteredFiles.IndexOf(currentFile);

            if (filteredIndex != -1)
            {
                var index = isNext
                    ? (filteredIndex < filteredFiles.Count - 1 ? filteredIndex + 1 : 0)
                    : (filteredIndex > 0 ? filteredIndex - 1 : filteredFiles.Count - 1);

                await PlayItem(filteredFiles[index]);
                return;
            }

            if (isNext) 
            {
                MarkLastItemOfCurrentType();
            }

            if (!isNext)
            {
                if (_filter is TeensyFilterType.Music && _lastSong is not null)
                {
                    MarkLastItemOfCurrentType();
                    await PlayItem(_lastSong);                    
                    _lastSong = null;
                    return;
                }
                else if (_filter is TeensyFilterType.Games && _lastGame is not null)
                {
                    MarkLastItemOfCurrentType();
                    await PlayItem(_lastGame);

                    _lastGame = null;
                    return;
                }
                else if (_filter is TeensyFilterType.Images && _lastImage is not null)
                {
                    MarkLastItemOfCurrentType();
                    await PlayItem(_lastImage);
                    _lastImage = null;
                    return;
                }
            }

            ILaunchableItem? candidate = null;

            var indexMap = unfilteredFiles
                .Select((file, index) => new { file, index })
                .ToDictionary(x => x.file, x => x.index);

            for (int x = 0; x < filteredFiles.Count; x++)
            {
                var f = filteredFiles[x];
                if (!indexMap.TryGetValue(f, out var fIndex)) continue;

                if (isNext && fIndex > unfilteredIndex)
                {
                    candidate = f;
                    break;
                }
                else if (!isNext)
                {
                    if (fIndex < unfilteredIndex) 
                    {
                        candidate = f;
                        continue;
                    }
                    if (fIndex > unfilteredIndex)
                    {
                        break;
                    }
                }
            }

            if (candidate is null)
            {
                var fallback = isNext ? filteredFiles.First() : filteredFiles.Last();
                await PlayItem(fallback);
                return;
            }
            await PlayItem(candidate);
        }

        private void MarkLastItemOfCurrentType()
        {
            if (_currentItem.Value is SongItem song)
            {
                _lastSong = song;
            }
            else if (_currentItem.Value is GameItem game)
            {
                _lastGame = game;
            }
            else if (_currentItem.Value is ImageItem image)
            {
                _lastImage = image;
            }
        }

        public async Task Pause()
        {
            if(_playerState.Value == TeensyRom.Player.PlayerState.Playing)
            {
                await mediator.Send(new ToggleMusicCommand());
            }
            timer.PauseTimer();
            _playerState.OnNext(TeensyRom.Player.PlayerState.Paused);
        }

        public async Task ResumeItem()
        {
            if (_playerState.Value == TeensyRom.Player.PlayerState.Paused)
            {
                timer.ResumeTimer();
                await mediator.Send(new ToggleMusicCommand());
                _playerState.OnNext(TeensyRom.Player.PlayerState.Playing);
                return;
            }
        }

        public async Task PlayItem(string path) 
        {
            var item = _items.FirstOrDefault(i => i.Path == path) 
                ?? throw new ArgumentException("Path was not found in playlist");

            await PlayItem(item);
        }

        public async Task PlayItem(ILaunchableItem item)
        {
            if (!_items.Contains(item))
            {
                _items.Add(item);
            }            
            _currentItem.OnNext(item);
            await StartStream();
        }

        private async Task StartStream() 
        {
            if(_currentItem.Value is null)
            {
                throw new Exception("You must set an item before a stream can start.");
            }            
            var result = await mediator.Send(new LaunchFileCommand(settingsService.GetSettings().StorageType, _currentItem.Value));

            if (result.LaunchResult is LaunchFileResultType.SidError or LaunchFileResultType.ProgramError)
            {
                _badFile.OnNext(_currentItem.Value);
            }            
            _playerState.OnNext(TeensyRom.Player.PlayerState.Playing);
            StartNewTimer();
        }

        private void StartNewTimer()
        {            
            _timerSubscription?.Dispose();

            if (_currentItem.Value is SongItem song && _songTimeOverride == false)
            {
                timer.StartNewTimer(song.PlayLength);
            }
            else if (_playTimer is not null)
            {
                timer.StartNewTimer(_playTimer.Value);
            }            
            _timerSubscription = timer.TimerComplete.Subscribe(async _ => await PlayNext());
        }

        public void SetPlaylist(List<ILaunchableItem> items)
        {
            _items.Clear();
            _items.AddRange(items);
        }

        public List<ILaunchableItem> GetPlaylist()
        {
            //TODO: Make immutable
            return _items;
        }

        public void SetPlayTimer(TimeSpan time)
        {
            _playTimer = time;
        }

        public void EnableSongTimeOverride() 
        {
            _songTimeOverride = true;
        }

        public void DisableSongTimeOverride()
        {
            _songTimeOverride = false;
        }
        public void ClearPlaytimer()
        {
            _playTimer = null;
        }

        public void Stop()
        {
            _playerState.OnNext(TeensyRom.Player.PlayerState.Stopped);
            _timerSubscription?.Dispose();
        }

        public ILaunchableItem? GetCurrent() 
        {
            return _currentItem.Value;
        }

        public void SetFilter(TeensyFilterType filter)
        {
            _filter = filter;
        }

        public void RemoveItem(ILaunchableItem item)
        {
            _items.Remove(item);
        }
    }
}