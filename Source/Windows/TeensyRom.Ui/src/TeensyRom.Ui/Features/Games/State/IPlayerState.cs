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
        Task DeleteFile(IFileItem file);
        Task<ILaunchableItem?> GetNext(ILaunchableItem currentFile, DirectoryState directoryState);
        Task StopFile();
        Task<ILaunchableItem?> GetPrevious(ILaunchableItem currentFile, DirectoryState directoryState);
    }
}