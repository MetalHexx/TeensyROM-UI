using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Controls.DirectoryTree;
using TeensyRom.Ui.Controls.PlayToolbar;
using TeensyRom.Ui.Features.Common.Models;
using TeensyRom.Ui.Features.Common.State;

namespace TeensyRom.Ui.Features.Music.State
{
    public interface IMusicState
    {
        IObservable<DirectoryNodeViewModel> DirectoryTree { get; }
        IObservable<ObservableCollection<IStorageItem>> DirectoryContent { get; }
        IObservable<SongItem> CurrentSong { get; }
        IObservable<TimeProgressViewModel> Time { get; }
        IObservable<LaunchItemState> LaunchState { get; }

        Task LoadDirectory(string path);
        Task RefreshDirectory(bool bustCache = true);
        Task LoadSong(ILaunchableItem song, bool clearHistory = true);
        Task SaveFavorite(ILaunchableItem song);
        Task DeleteFile(IFileItem file);
        Task PlayNext();
        Task PlayPrevious();
        Task ToggleMusic();
        Task<SongItem?> PlayRandom();
        Unit SearchMusic(string keyword);
        Task ClearSearch();
        Unit ToggleShuffleMode();
        Task CacheAll();
    }
}
