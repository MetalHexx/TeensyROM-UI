using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TeensyRom.Core.Music;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Controls.DirectoryTree;
using TeensyRom.Ui.Features.Common.Models;

namespace TeensyRom.Ui.Features.Discover.State.Player
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
        IObservable<LaunchedFileResult> LaunchedFile { get; }
        IObservable<ILaunchableItem> SelectedFile { get; }
        IObservable<int> TotalPages { get; }
        IObservable<StorageScope> CurrentScope { get; }
        IObservable<string> CurrentScopePath { get; }
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
        Task PlaySubtune(int subtuneIndex);
        Task<ILaunchableItem?> PlayRandom();
        void UpdateHistory(ILaunchableItem fileToLoad);
        Unit PreviousPage();
        Task RefreshDirectory(bool bustCache = true);
        Unit SearchFiles(string keyword);
        Unit SetPageSize(int pageSize);
        Unit SelectFile(ILaunchableItem file);
        Task StopFile();
        Unit ToggleShuffleMode();
        bool TryTransitionTo(Type nextStateType);
        Task SwitchFilterAndLaunch(TeensyFilter filter);
        Task StoreFiles(IEnumerable<DragNDropFile> files);
        Task AutoStoreFiles(IEnumerable<FileTransferItem> files);
        void SetScope(StorageScope scope);
        void SetScopePath(string path);
        string GetScopePath();
        Task SetSpeed(double percentage, MusicSpeedCurveTypes curveType);
        Task RestartSong();
        Task RestartSubtune(int subtuneIndex);
        Task Mute(bool voice1, bool voice2, bool voice3);        
    }
}