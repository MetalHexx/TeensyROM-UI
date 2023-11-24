using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Features.Common.Models;
using TeensyRom.Ui.Features.Music.State;
using TeensyRom.Ui.Features.NavigationHost;

namespace TeensyRom.Ui.Features.Music.SongList
{
    public class SongListViewModel: ReactiveObject, IDisposable
    {
        [ObservableAsProperty] public ObservableCollection<StorageItem> DirectoryContent { get; }
        [ObservableAsProperty] public bool ShowProgress { get; }
        public ReactiveCommand<SongItem, bool> PlayCommand { get; set; }
        public ReactiveCommand<DirectoryItem, Unit> LoadDirectoryCommand { get; set; }

        private readonly IMusicState _musicState;
        private IDisposable _directoryTreeSubscription;

        public SongListViewModel(IMusicState musicState)
        {
            _musicState = musicState;

            _musicState.DirectoryLoading
                .ToPropertyEx(this, x => x.ShowProgress);
           
            PlayCommand = ReactiveCommand.Create<SongItem, bool>(song =>
                musicState.LoadSong(song), outputScheduler: RxApp.MainThreadScheduler);

            LoadDirectoryCommand = ReactiveCommand.CreateFromObservable<DirectoryItem, Unit>(directory =>
                musicState.LoadDirectory(directory.Path), outputScheduler: RxApp.MainThreadScheduler);

            _musicState.DirectoryContent.ToPropertyEx(this, x => x.DirectoryContent);
        }
        public void Dispose()
        {
            _directoryTreeSubscription?.Dispose();
        }
    }
}