using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Controls.DirectoryTree;
using TeensyRom.Ui.Features.Common.Models;

namespace TeensyRom.Ui.Features.Music.State
{
    public interface IMusicState
    {
        IObservable<DirectoryNodeViewModel> DirectoryTree { get; }
        IObservable<ObservableCollection<StorageItem>> DirectoryContent { get; }
        IObservable<bool> DirectoryLoading { get; }
        IObservable<SongItem> CurrentSong { get; }
        IObservable<SongMode> CurrentSongMode { get; }
        IObservable<TimeSpan> CurrentSongTime { get; }
        IObservable<PlayState> CurrentPlayState { get; }

        Task LoadDirectory(string path);
        Task RefreshDirectory(bool bustCache = true);
        Task<bool> LoadSong(SongItem song);
        Task<bool> SaveFavorite(SongItem song);
        Task DeleteFile(FileItem file);
        Task PlayNext();
        Task PlayPrevious();
        Task ToggleMusic();
        Task PlayRandom();
        Unit ToggleShuffleMode();
    }
}
