using MediatR;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using TeensyRom.Core.Common;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Core.Settings;
using TeensyRom.Ui.Core.Storage.Services;
using TeensyRom.Ui.Features.Discover.State.Directory;
using TeensyRom.Ui.Services;

namespace TeensyRom.Ui.Features.Discover.State.Player
{
    public abstract class PlayerState : IPlayerState, IDisposable
    {
        protected BehaviorSubject<PlayState> _playState = new(PlayState.Stopped);
        protected readonly ICachedStorageService _storage;
        protected readonly ISettingsService _settingsService;
        protected readonly ILaunchHistory _launchHistory;
        protected readonly ISnackbarService _alert;
        protected readonly PlayerContext _playerContext;
        protected readonly IMediator _mediator;
        protected IDisposable? _settingsSubscription;
        protected TeensySettings _settings = null!;

        public PlayerState(PlayerContext playerContext, IMediator mediator, ICachedStorageService storage, ISettingsService settingsService, ILaunchHistory launchHistory, ISnackbarService alert)
        {
            _storage = storage;
            _settingsService = settingsService;
            _launchHistory = launchHistory;
            _alert = alert;
            _playerContext = playerContext;
            _mediator = mediator;
            _playerContext = playerContext;
            _settingsSubscription = settingsService.Settings.Subscribe(settings => _settings = settings);
        }

        public abstract bool CanTransitionTo(Type nextStateType);

        public virtual Task<ILaunchableItem?> GetNext(ILaunchableItem currentFile, TeensyFilterType filter, DirectoryState directoryState) => throw new TeensyStateException(InvalidStateExceptionMessage);

        public virtual Task<ILaunchableItem?> GetPrevious(ILaunchableItem currentFile, TeensyFilterType filter, DirectoryState directoryState) => throw new TeensyStateException(InvalidStateExceptionMessage);

        protected string InvalidStateExceptionMessage => $"Cannot perform this operation from: {GetType().Name}";

        public void Dispose()
        {
            _settingsSubscription?.Dispose();
        }
    }
}
