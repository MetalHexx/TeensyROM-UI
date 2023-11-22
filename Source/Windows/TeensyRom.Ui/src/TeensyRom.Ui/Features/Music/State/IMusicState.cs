using System;
using System.Collections.Generic;
using TeensyRom.Ui.Features.Common.Models;

namespace TeensyRom.Ui.Features.Music.State
{
    public interface IMusicState
    {
        IObservable<IEnumerable<StorageItem>> DirectoryContent { get; }
        IObservable<SongItem> CurrentSong { get; }
        IObservable<SongMode> CurrentSongMode { get; }
        IObservable<TimeSpan> CurrentSongTime { get; }
        IObservable<PlayState> CurrentPlayState { get; }

        bool LoadDirectory(string path);
        bool LoadSong(SongItem song);
        bool PlayNext();
        bool PlayPrevious();
        bool ToggleMusic();
    }
}
