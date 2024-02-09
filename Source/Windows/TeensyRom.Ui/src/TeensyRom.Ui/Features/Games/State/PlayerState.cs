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
using TeensyRom.Ui.Features.Music.State;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Services;

namespace TeensyRom.Ui.Features.Games.State
{
    public abstract class PlayerState : IPlayerState
    {
        public IObservable<GameItem> SelectedGame => _selectedGame.AsObservable();
        public IObservable<PlayPausedState> PlayState => _gameState.AsObservable();
        protected BehaviorSubject<GameItem> _selectedGame = new(null!);
        protected BehaviorSubject<PlayPausedState> _gameState = new(PlayPausedState.Stopped);
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
        }

        public abstract bool CanTransitionTo(Type nextStateType);
        public virtual Task ClearSearch() => throw new TeensyStateException(InvalidStateExceptionMessage);

        public virtual async Task DeleteFile(GameItem game) => await _storage.DeleteFile(game, _settings.TargetType);

        public virtual Task StopGame()
        {
            _gameState.OnNext(PlayPausedState.Stopped);
            return _mediator.Send(new ResetCommand());
        }

        public virtual Task<GameItem?> GetNext(GameItem currentGame, DirectoryState directoryState) => throw new TeensyStateException(InvalidStateExceptionMessage);

        public virtual Task<GameItem?> GetPrevious(GameItem currentGame, DirectoryState directoryState) => throw new TeensyStateException(InvalidStateExceptionMessage);

        public virtual Task<GameItem?> PlayRandom() => throw new TeensyStateException(InvalidStateExceptionMessage);

        public virtual Task RefreshDirectory(bool bustCache = true) => throw new TeensyStateException(InvalidStateExceptionMessage);

        protected string InvalidStateExceptionMessage => $"Cannot perform this operation from: {GetType().Name}";
    }
}
