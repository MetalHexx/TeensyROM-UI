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
using TeensyRom.Ui.Features.Music.MusicTree;
using TeensyRom.Ui.Features.Music.PlayToolbar;
using TeensyRom.Ui.Features.Music.SongList;
using TeensyRom.Ui.Features.Music.State;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Helpers.ViewModel;

namespace TeensyRom.Ui.Features.Music
{
    public class MusicViewModel : FeatureViewModelBase, IDisposable
    {
        private readonly IMusicState _musicState;
        private IDisposable _loadSongsSubscription;

        [ObservableAsProperty] public bool ShowPlayToolbar { get; set; }
        [Reactive] public PlayToolbarViewModel PlayToolBar { get; set; }       
        [Reactive] public SongListViewModel SongList { get; set; }
        [Reactive] public MusicTreeViewModel MusicTree { get; set; }
        public ReactiveCommand<Unit, Unit> RefreshCommand { get; set; }


        public MusicViewModel(IMusicState musicState, ISerialPortState serialState, ISettingsService settings, INavigationService nav, PlayToolbarViewModel playToolBar, SongListViewModel songList, MusicTreeViewModel musicTree) 
        {
            FeatureTitle = "Music";
            _musicState = musicState;
            PlayToolBar = playToolBar;
            SongList = songList;
            MusicTree = musicTree;
            _musicState.CurrentSong
                .Select(s => s is null)
                .ToPropertyEx(this, x => x.ShowPlayToolbar);

            RefreshCommand = ReactiveCommand.CreateFromObservable<Unit, Unit>( _ => musicState.RefreshDirectory());

            _loadSongsSubscription = serialState.IsConnected
                .Where(isConnected => isConnected is true)
                .CombineLatest(settings.Settings, (isConnected, settings) => settings)
                .CombineLatest(nav.SelectedNavigationView, (settings, currentNav) => (settings, currentNav))
                .Where(sn => sn.currentNav?.Type == NavigationLocation.Music)
                //.Where(_ => DirectoryContent.Count == 0)
                .Subscribe(sn => LoadSongs(sn.settings.GetFileTypePath(TeensyFileType.Sid)));
        }

        public Unit LoadSongs(string path)
        {
            _musicState.LoadDirectory(path);
            return Unit.Default;
        }

        public void Dispose()
        {
            _loadSongsSubscription?.Dispose();
        }
    }
}
