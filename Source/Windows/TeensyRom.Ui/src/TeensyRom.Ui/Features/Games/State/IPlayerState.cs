using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Ui.Features.Games.State
{
    public interface IPlayerState
    {
        IObservable<DirectoryState> DirectoryState { get; }
        IObservable<GameItem> LaunchedGame { get; }
        IObservable<GameItem> SelectedGame { get; }

        Task CacheAll();
        bool CanTransitionTo(Type nextStateType);
        Task ClearSearch();
        Task DeleteFile(GameItem game);
        void Handle();
        Task LoadDirectory(string path, string? filePathToSelect = null);
        Task PlayGame(GameItem game);
        Unit NextPage();
        Task PlayNext();
        Task StopGame();
        Task PlayPrevious();
        Task<GameItem?> PlayRandom();
        Unit PreviousPage();
        Task RefreshDirectory(bool bustCache = true);
        Task SaveFavorite(GameItem game);
        Unit SearchGames(string searchText);
        Unit SetPageSize(int pageSize);
        Unit SetSelectedGame(GameItem game);
    }
}