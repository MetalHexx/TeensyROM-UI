using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Controls.DirectoryTree;

namespace TeensyRom.Ui.Features.Games.State
{
    public interface IPlayerContext
    {
        IObservable<int> CurrentPage { get; }
        IObservable<PlayPausedState> PlayState { get; }
        IObservable<PlayerState> CurrentState { get; }
        IObservable<ObservableCollection<StorageItem>> DirectoryContent { get; }
        IObservable<DirectoryNodeViewModel?> DirectoryTree { get; }
        IObservable<bool> PagingEnabled { get; }
        IObservable<GameItem> LaunchedGame { get; }
        IObservable<GameItem> SelectedGame { get; }
        IObservable<int> TotalPages { get; }
        Task CacheAll();
        Task ClearSearch();
        Task DeleteFile(GameItem file);
        Task LoadDirectory(string path, string? filePathToSelect = null);
        Task PlayGame(GameItem game);
        Unit NextPage();
        Task PlayNext();
        Task PlayPrevious();
        Task<GameItem?> PlayRandom();
        Unit PreviousPage();
        Task RefreshDirectory(bool bustCache = true);
        Task SaveFavorite(GameItem game);
        Unit SearchGames(string keyword);
        Unit SetPageSize(int pageSize);
        Unit SetSelectedGame(GameItem game);
        Task StopGame();
        Unit ToggleShuffleMode();
        bool TryTransitionTo(Type nextStateType);
    }
}