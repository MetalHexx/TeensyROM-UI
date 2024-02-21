using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Controls.DirectoryTree;
using TeensyRom.Ui.Features.Common.State;

namespace TeensyRom.Ui.Features.Games.State
{
    public interface IPlayerContext
    {
        IObservable<string> CurrentPath { get; }
        IObservable<int> CurrentPage { get; }
        IObservable<PlayState> PlayingState { get; }
        IObservable<PlayerState> CurrentState { get; }
        IObservable<ObservableCollection<IStorageItem>> DirectoryContent { get; }
        IObservable<DirectoryNodeViewModel?> DirectoryTree { get; }
        IObservable<bool> PagingEnabled { get; }
        IObservable<ILaunchableItem> LaunchedGame { get; }
        IObservable<ILaunchableItem> SelectedGame { get; }
        IObservable<int> TotalPages { get; }
        Task CacheAll();
        Task ClearSearch();
        Task DeleteFile(IFileItem file);
        Task LoadDirectory(string path);
        Task LoadDirectory(string path, string? filePathToSelect = null);
        Task PlayGame(ILaunchableItem game);
        Task ToggleGame();
        Unit NextPage();
        Task PlayNext();
        Task PlayPrevious();
        Task<ILaunchableItem?> PlayRandom();
        Unit PreviousPage();
        Task RefreshDirectory(bool bustCache = true);
        Task SaveFavorite(ILaunchableItem game);
        Unit SearchGames(string keyword);
        Unit SetPageSize(int pageSize);
        Unit SetSelectedGame(ILaunchableItem game);
        Task StopGame();
        Unit ToggleShuffleMode();
        bool TryTransitionTo(Type nextStateType);
    }
}