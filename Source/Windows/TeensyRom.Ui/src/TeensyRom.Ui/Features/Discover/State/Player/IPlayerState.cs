using System;
using System.Threading.Tasks;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Settings;
using TeensyRom.Ui.Features.Discover.State.Directory;

namespace TeensyRom.Ui.Features.Discover.State.Player
{
    public interface IPlayerState
    {
        bool CanTransitionTo(Type nextStateType);
        Task<LaunchableItem?> GetNext(LaunchableItem currentFile, TeensyFilterType filter, DirectoryState directoryState);
        Task<LaunchableItem?> GetPrevious(LaunchableItem currentFile, TeensyFilterType filter, DirectoryState directoryState);
    }
}