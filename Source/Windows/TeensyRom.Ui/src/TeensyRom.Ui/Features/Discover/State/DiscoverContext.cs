using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Assets.Tools.Vice;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Core.Storage.Services;
using TeensyRom.Ui.Features.Common.State;
using TeensyRom.Ui.Features.Common.State.Player;
using TeensyRom.Ui.Features.Common.State.Progress;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Services;

namespace TeensyRom.Ui.Features.Discover.State
{
    public interface IDiscoverContext : IPlayerContext { }
    public class DiscoverContext : PlayerContext, IDiscoverContext
    {
        private TimeSpan _currentTime = TimeSpan.Zero;
        public DiscoverContext(IMediator mediator, ICachedStorageService storage, ISettingsService settingsService, ILaunchHistory launchHistory, IFileWatchService watchService, ID64Extractor d64Extractor, ISnackbarService alert, ISerialStateContext serialContext, INavigationService nav, IDiscoveryTreeState tree, IDiscoverViewConfig config, IProgressTimer timer, ILoggingService log)
            : base(mediator, storage, settingsService, launchHistory, watchService, d64Extractor, alert, serialContext, nav, tree, config, log)
        {
            timer.CurrentTime
                .Subscribe(time => _currentTime = time);
        }

        public override async Task TogglePlay()
        {
            if (_launchedFile.Value is SongItem)
            {
                var result = await _mediator.Send(new ToggleMusicCommand());

                if (result.IsBusy)
                {
                    _alert.Enqueue("Toggle music failed. Re-launching the current song.");
                    _playingState.OnNext(PlayState.Playing);
                    await PlayFile(_launchedFile.Value!);
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
            await PlayFile(_launchedFile.Value!);
            return;
        }

        public override Task PlayPrevious()
        {
            if (_launchedFile.Value is SongItem && _currentTime >= TimeSpan.FromSeconds(3))
            {
                return base.PlayFile(_launchedFile.Value!);
            }
            if (_launchedFile.Value is ILaunchableItem)
            {
                return base.PlayPrevious();
            }            
            return PlayFile(_launchedFile.Value!);
        }
    }
}
