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
using TeensyRom.Ui.Features.Global;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Services;

namespace TeensyRom.Ui.Features.Music.State
{
    public interface IMusicPlayerContext : IPlayerContext { }
    public sealed class MusicPlayerContext : PlayerContext, IMusicPlayerContext
    {
        private TimeSpan _currentTime = TimeSpan.Zero;
        public MusicPlayerContext(IMediator mediator, ICachedStorageService storage, ISettingsService settingsService, ILaunchHistory launchHistory, ISnackbarService alert, ISerialStateContext serialContext, INavigationService nav, IMusicTreeState tree, IMusicViewConfig config, IGlobalState globalState, IProgressTimer timer)
            : base(mediator, storage, settingsService, launchHistory, alert, serialContext, nav, tree, config) 
        {

            globalState.ProgramLaunched
                .Where(p => p is not null)
                .Subscribe(_ => _playingState.OnNext(PlayState.Paused));

            timer.CurrentTime
                .Subscribe(time => _currentTime = time);
        }

        public override async Task TogglePlay()
        {
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
            if(_currentTime <= TimeSpan.FromSeconds(3))
            {
                return base.PlayPrevious();
            }
            else
            {
                return PlayFile(_launchedFile.Value!);
            }
        }
    }
}
