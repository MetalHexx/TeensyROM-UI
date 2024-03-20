using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Settings;
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
        public DiscoverContext(IMediator mediator, ICachedStorageService storage, ISettingsService settingsService, ILaunchHistory launchHistory, ISnackbarService alert, ISerialStateContext serialContext, INavigationService nav, IDiscoveryTreeState tree, IDiscoverViewConfig config, IProgressTimer timer)
            : base(mediator, storage, settingsService, launchHistory, alert, serialContext, nav, tree, config)
        {
            timer.CurrentTime
                .Subscribe(time => _currentTime = time);
        }

        public override async Task TogglePlay()
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
                await PlayFile(_launchedFile.Value!);
                return;
            }
            var result = await _mediator.Send(new ToggleMusicCommand());

            if (result.IsBusy)
            {
                _alert.Enqueue("Toggle music failed. Re-launching the current song.");
                await PlayFile(_launchedFile.Value!);
                return;
            }
        }

        public override Task PlayPrevious()
        {
            if (_launchedFile.Value is GameItem)
            {
                return base.PlayPrevious();
            }
            if (_currentTime <= TimeSpan.FromSeconds(3))
            {
                return base.PlayPrevious();
            }
            return PlayFile(_launchedFile.Value!);
        }
    }
}
