﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Features.Common.State.Directory;
using TeensyRom.Ui.Features.Games.State.Directory;

namespace TeensyRom.Ui.Features.Common.State.Player
{
    public interface IPlayerState
    {
        bool CanTransitionTo(Type nextStateType);
        Task<ILaunchableItem?> GetNext(ILaunchableItem currentFile, DirectoryState directoryState);
        Task<ILaunchableItem?> GetPrevious(ILaunchableItem currentFile, DirectoryState directoryState);
    }
}