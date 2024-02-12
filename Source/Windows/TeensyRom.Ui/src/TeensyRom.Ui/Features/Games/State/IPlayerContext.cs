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
        IObservable<FileItem> LaunchedGame { get; }
        IObservable<FileItem> SelectedGame { get; }
        IObservable<int> TotalPages { get; }
        Task CacheAll();
        Task ClearSearch();
        Task DeleteFile(FileItem file);
        Task LoadDirectory(string path, string? filePathToSelect = null);
        Task PlayGame(FileItem game);
        Unit NextPage();
        Task PlayNext();
        Task PlayPrevious();
        Task<FileItem?> PlayRandom();
        Unit PreviousPage();
        Task RefreshDirectory(bool bustCache = true);
        Task SaveFavorite(FileItem game);
        Unit SearchGames(string keyword);
        Unit SetPageSize(int pageSize);
        Unit SetSelectedGame(FileItem game);
        Task StopGame();
        Unit ToggleShuffleMode();
        bool TryTransitionTo(Type nextStateType);
    }
}