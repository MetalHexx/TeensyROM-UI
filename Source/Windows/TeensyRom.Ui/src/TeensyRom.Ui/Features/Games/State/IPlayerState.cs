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
        Task DeleteFile(FileItem game);
        Task<FileItem?> GetNext(FileItem currentGame, DirectoryState directoryState);
        Task StopGame();
        Task<FileItem?> GetPrevious(FileItem currentGame, DirectoryState directoryState);
        Task RefreshDirectory(bool bustCache = true);
    }
}