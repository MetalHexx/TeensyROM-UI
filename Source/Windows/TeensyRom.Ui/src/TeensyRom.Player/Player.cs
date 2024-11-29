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
        private int _currentIndex;
        private TimeSpan? _playTimer = null;
        private bool _songTimeOverride = false;

        public async Task PlayNext()
        {
            await PlayNextOrPrevious(true);
        }

        public async Task PlayPrevious()
        {
            await PlayNextOrPrevious(false);
        }

        private async Task PlayNextOrPrevious(bool isNext)
        {
            var unfilteredFiles = _items;

            if (unfilteredFiles.Count == 0) return;

            var currentFile = unfilteredFiles.FirstOrDefault(f => f.Id == _currentItem.Value?.Id);

            if (currentFile is null) return;

            var unfilteredIndex = unfilteredFiles.IndexOf(currentFile);

            var filteredFiles = unfilteredFiles
                .Where(f => StorageHelper.GetFileTypes(_filter)
                    .Any(t => f.FileType == t))
                .ToList();

            if (filteredFiles.Count == 0) return;

            if (unfilteredIndex > filteredFiles.Count - 1)
            {
                await PlayAndPublish(filteredFiles.First());
                return;
            }

            var filteredIndex = filteredFiles.IndexOf(currentFile);

            if (filteredIndex != -1)
            {
                var index = isNext
                    ? (filteredIndex < filteredFiles.Count - 1 ? filteredIndex + 1 : 0)
                    : (filteredIndex > 0 ? filteredIndex - 1 : filteredFiles.Count - 1);

                await PlayAndPublish(filteredFiles[index]);
                return;
            }

            ILaunchableItem? candidate = null;

            for (int x = 0; x < filteredFiles.Count; x++)
            {
                var f = filteredFiles[x];
                var fIndex = unfilteredFiles.IndexOf(f);

                if (isNext && fIndex > unfilteredIndex)
                {
                    candidate = f;
                    continue;
                }
                else if (!isNext && fIndex < unfilteredIndex)
                {
                    candidate = f;
                    break;
                }
            }

            if (candidate is null)
            {
                var fallback = isNext ? filteredFiles.First() : filteredFiles.Last();
                await PlayAndPublish(fallback);
                return;
            }

            await PlayAndPublish(candidate);
        }

        private async Task PlayAndPublish(ILaunchableItem item)
        {
            SetPlayItem(item);
            PublishItem(item);
            await PlayItem();
        }

        private void PublishItem(ILaunchableItem item)
        {
            _currentItem.OnNext(item);
            _currentIndex = _items.IndexOf(item);
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

        public async Task PlayItem()
        {
            if(_currentItem.Value is null)
            {
                throw new Exception("You must set a playlist before playing an item");
            }
            if (_playerState.Value == TeensyRom.Player.PlayerState.Paused)
            {                
                timer.ResumeTimer();
                await mediator.Send(new ToggleMusicCommand());
                _playerState.OnNext(TeensyRom.Player.PlayerState.Playing);
            }
            if(_playerState.Value == TeensyRom.Player.PlayerState.Stopped)
            {
                if (_currentItem.Value is SongItem song && _songTimeOverride == false)
                {
                    timer.StartNewTimer(song.PlayLength);
                }
                else if (_playTimer is not null)
                {
                    timer.StartNewTimer(_playTimer.Value);
                }
                var result = await mediator.Send(new LaunchFileCommand(settingsService.GetSettings().StorageType, _currentItem.Value));
                
                if (result.LaunchResult is LaunchFileResultType.SidError or LaunchFileResultType.ProgramError) 
                {
                    _badFile.OnNext(_currentItem.Value);
                }
                _playerState.OnNext(TeensyRom.Player.PlayerState.Playing);
            }
        }

        public void SetPlayItem(ILaunchableItem item)
        {
            if(_items.Contains(item))
            {
                _currentIndex = _items.IndexOf(item);
            }
            _currentItem.OnNext(item);
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

        public void ClearSongTimeOverride()
        {
            _songTimeOverride = false;
        }

        public void ClearPlaytimer()
        {
            _playTimer = null;
        }

        public async Task Stop()
        {
            _playerState.OnNext(TeensyRom.Player.PlayerState.Stopped);
            _timerSubscription?.Dispose();
            await mediator.Send(new ResetCommand());
        }

        public ILaunchableItem GetCurrent() 
        {
            return _items[_currentIndex];
        }

        public void SetFilter(TeensyFilterType filter)
        {
            _filter = filter;
        }
    }
}
