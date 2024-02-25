using DynamicData;
using MaterialDesignThemes.Wpf;
using MediatR;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Commands.File.LaunchFile;
using TeensyRom.Core.Common;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Core.Storage.Services;
using TeensyRom.Ui.Controls.DirectoryTree;
using TeensyRom.Ui.Features.Common.State.Directory;
using TeensyRom.Ui.Features.Music.State;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Services;
using PlayState = TeensyRom.Ui.Features.Common.State.PlayState;

namespace TeensyRom.Ui.Features.Common.State.Player
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

        public virtual Task<ILaunchableItem?> GetNext(ILaunchableItem currentFile, DirectoryState directoryState) => throw new TeensyStateException(InvalidStateExceptionMessage);

        public virtual Task<ILaunchableItem?> GetPrevious(ILaunchableItem currentFile, DirectoryState directoryState) => throw new TeensyStateException(InvalidStateExceptionMessage);

        protected string InvalidStateExceptionMessage => $"Cannot perform this operation from: {GetType().Name}";

        public void Dispose()
        {
            _settingsSubscription?.Dispose();
        }
    }
}
