using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Features.Games.State.Directory;

namespace TeensyRom.Ui.Features.Games.State
{
    public interface IPlayerState
    {
        bool CanTransitionTo(Type nextStateType);
        Task ClearSearch();
        Task DeleteFile(IFileItem game);
        Task<ILaunchableItem?> GetNext(ILaunchableItem currentGame, DirectoryState directoryState);
        Task StopGame();
        Task<ILaunchableItem?> GetPrevious(ILaunchableItem currentGame, DirectoryState directoryState);
    }
}