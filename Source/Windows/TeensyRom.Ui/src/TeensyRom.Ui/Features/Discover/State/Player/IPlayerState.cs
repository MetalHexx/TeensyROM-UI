using System;
using System.Threading.Tasks;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Core.Settings;
using TeensyRom.Ui.Features.Discover.State.Directory;

namespace TeensyRom.Ui.Features.Discover.State.Player
{
    public interface IPlayerState
    {
        bool CanTransitionTo(Type nextStateType);
        Task<ILaunchableItem?> GetNext(ILaunchableItem currentFile, TeensyFilterType filter, DirectoryState directoryState);
        Task<ILaunchableItem?> GetPrevious(ILaunchableItem currentFile, TeensyFilterType filter, DirectoryState directoryState);
    }
}