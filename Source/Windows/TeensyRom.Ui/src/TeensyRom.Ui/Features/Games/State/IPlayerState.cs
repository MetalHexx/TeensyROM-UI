using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Ui.Features.Games.State
{
    public interface IPlayerState
    {
        IObservable<GameItem> SelectedGame { get; }
        bool CanTransitionTo(Type nextStateType);
        Task ClearSearch();
        Task DeleteFile(GameItem game);
        Task<GameItem?> GetNext(GameItem currentGame, DirectoryState directoryState);
        Task StopGame();
        Task<GameItem?> GetPrevious(GameItem currentGame, DirectoryState directoryState);
        Task<GameItem?> PlayRandom();
        Task RefreshDirectory(bool bustCache = true);
    }
}