//using MediatR;
//using System;
//using TeensyRom.Core.Serial.State;
//using TeensyRom.Core.Settings;
//using TeensyRom.Core.Storage.Services;
//using TeensyRom.Ui.Features.NavigationHost;
//using TeensyRom.Ui.Services;

//namespace TeensyRom.Ui.Features.Games.State
//{
//    public class PausedState : PlayerState
//    {
//        public PausedState(FilePlayer playerContext, IMediator mediator, ICachedStorageService storage, ISettingsService settingsService, ILaunchHistory launchHistory, ISnackbarService alert, ISerialStateContext serialContext, INavigationService nav) : base(playerContext, mediator, storage, settingsService, launchHistory, alert, serialContext, nav) { }

//        public override bool CanTransitionTo(Type nextStateType)
//        {
//            throw new NotImplementedException();
//        }

//        public override void Handle(FilePlayer player)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
