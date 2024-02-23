using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Core.Storage.Services;
using TeensyRom.Ui.Features.Games.State.Directory;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Services;

namespace TeensyRom.Ui.Features.Games.State
{
    public interface IGamePlayerContext : IPlayerContext { }
    public sealed class GamePlayerContext : PlayerContext, IGamePlayerContext
    {
        public GamePlayerContext(IMediator mediator, ICachedStorageService storage, ISettingsService settingsService, ILaunchHistory launchHistory, ISnackbarService alert, ISerialStateContext serialContext, INavigationService nav, IGameDirectoryTreeState tree, IGameViewConfig config)
            : base(mediator, storage, settingsService, launchHistory, alert, serialContext, nav, tree, config.FileTypes) { }
    }
}
