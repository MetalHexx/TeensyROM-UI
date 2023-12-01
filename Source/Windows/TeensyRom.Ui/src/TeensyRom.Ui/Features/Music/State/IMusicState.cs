using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Features.Common.Models;

namespace TeensyRom.Ui.Features.Music.State
{
    public interface IMusicState
    {
        IObservable<DirectoryItem> DirectoryTree { get; }
        IObservable<ObservableCollection<StorageItem>> DirectoryContent { get; }
        IObservable<bool> DirectoryLoading { get; }
        IObservable<SongItem> CurrentSong { get; }
        IObservable<SongMode> CurrentSongMode { get; }
        IObservable<TimeSpan> CurrentSongTime { get; }
        IObservable<PlayState> CurrentPlayState { get; }

        IObservable<Unit> LoadDirectory(string path);
        Unit LoadDirectorySync(string path);
        IObservable<Unit> RefreshDirectory();
        bool LoadSong(SongItem song);
        bool SaveFavorite(SongItem song);
        void PlayNext();
        void PlayPrevious();
        bool ToggleMusic();
    }
}
