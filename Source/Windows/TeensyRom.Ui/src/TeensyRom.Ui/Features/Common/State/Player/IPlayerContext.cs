using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Documents;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Controls.DirectoryTree;
using TeensyRom.Ui.Features.Common.Models;
using TeensyRom.Ui.Features.Common.State;

namespace TeensyRom.Ui.Features.Common.State.Player
{
    public interface IPlayerContext
    {
        TeensyFileType[] GetFileTypes();
        IObservable<string> CurrentPath { get; }
        IObservable<int> CurrentPage { get; }
        IObservable<PlayState> PlayingState { get; }
        IObservable<PlayerState> CurrentState { get; }
        IObservable<ObservableCollection<IStorageItem>> DirectoryContent { get; }
        IObservable<DirectoryNodeViewModel?> DirectoryTree { get; }
        IObservable<bool> PagingEnabled { get; }
        IObservable<ILaunchableItem> LaunchedFile { get; }
        IObservable<ILaunchableItem> SelectedFile { get; }
        IObservable<int> TotalPages { get; }
        Task CacheAll();
        Task ClearSearch();
        Task DeleteFile(IFileItem file);
        Task LoadDirectory(string path);
        Task LoadDirectory(string path, string? filePathToSelect = null);
        Task PlayFile(ILaunchableItem file);
        Task TogglePlay();
        Unit NextPage();
        Task PlayNext();
        Task PlayPrevious();
        Task<ILaunchableItem?> PlayRandom();
        void UpdateHistory(ILaunchableItem fileToLoad);
        Unit PreviousPage();
        Task RefreshDirectory(bool bustCache = true);
        Task SaveFavorite(ILaunchableItem file);
        Unit SearchFiles(string keyword);
        Unit SetPageSize(int pageSize);
        Unit SelectFile(ILaunchableItem file);
        Task StopFile();
        Unit ToggleShuffleMode();
        bool TryTransitionTo(Type nextStateType);     
        Task SwitchFilter(TeensyFilter filter);
        Task StoreFiles(IEnumerable<FileCopyItem> files);
        Task AutoStoreFiles(IEnumerable<TeensyFileInfo> files);
    }
}