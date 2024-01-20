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
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Controls.DirectoryTree;
using TeensyRom.Ui.Features.Common.Models;
using TeensyRom.Ui.Features.Files.State;
using TeensyRom.Ui.Features.Music.PlayToolbar;
using TeensyRom.Ui.Features.Music.Search;
using TeensyRom.Ui.Features.Music.SongList;
using TeensyRom.Ui.Features.Music.State;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Helpers.ViewModel;

namespace TeensyRom.Ui.Features.Music
{
    public class MusicViewModel : FeatureViewModelBase
    {
        private readonly IMusicState _musicState;

        [ObservableAsProperty] public bool ShowPlayToolbar { get; set; }
        [Reactive] public PlayToolbarViewModel PlayToolBar { get; set; }       
        [Reactive] public SongListViewModel SongList { get; set; }
        [Reactive] public SearchMusicViewModel Search { get; set; }
        [Reactive] public DirectoryTreeViewModel MusicTree { get; set; }
        public ReactiveCommand<Unit, Unit> RefreshCommand { get; set; }
        public ReactiveCommand<Unit, Unit> PlayRandomCommand { get; set; }
        public ReactiveCommand<Unit, Unit> CacheAllCommand { get; set; }

        public MusicViewModel(IMusicState musicState, ISerialStateContext serialContext, ISettingsService settings, INavigationService nav, PlayToolbarViewModel playToolBar, SongListViewModel songList, SearchMusicViewModel search)
        {
            FeatureTitle = "Music";
            _musicState = musicState;
            PlayToolBar = playToolBar;
            SongList = songList;
            Search = search;
            _musicState.CurrentSong
                .Select(s => s is null)
                .ToPropertyEx(this, x => x.ShowPlayToolbar);

            RefreshCommand = ReactiveCommand.CreateFromTask<Unit>(_ => musicState.RefreshDirectory());
            PlayRandomCommand = ReactiveCommand.CreateFromTask<Unit>(_ => musicState.PlayRandom());
            CacheAllCommand = ReactiveCommand.CreateFromTask<Unit>(_ => musicState.CacheAll());

            MusicTree = new(musicState.DirectoryTree)
            {
                DirectorySelectedCommand = ReactiveCommand.CreateFromTask<DirectoryNodeViewModel>(async (directory) =>
                await musicState.LoadDirectory(directory.Path), outputScheduler: RxApp.MainThreadScheduler)
            };
        }
    }
}
