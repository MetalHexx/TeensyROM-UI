using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Controls.DirectoryTree;
using TeensyRom.Ui.Controls.PlayToolbar;
using TeensyRom.Ui.Features.Common.Models;
using TeensyRom.Ui.Features.Files.State;
using TeensyRom.Ui.Features.Global;
using TeensyRom.Ui.Features.Music.Search;
using TeensyRom.Ui.Features.Music.SongList;
using TeensyRom.Ui.Features.Music.State;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Helpers.ViewModel;
using TeensyRom.Ui.Services;

namespace TeensyRom.Ui.Features.Music
{
    public class MusicViewModel : FeatureViewModelBase
    {
        [ObservableAsProperty] public bool MusicAvailable { get; set; }
        [ObservableAsProperty] public bool ShowPlayToolbar { get; set; }

        [Reactive] public PlayToolbarViewModel PlayToolBar { get; set; }       
        [Reactive] public SongListViewModel SongList { get; set; }
        [Reactive] public SearchMusicViewModel Search { get; set; }
        [Reactive] public DirectoryTreeViewModel MusicTree { get; set; }
        
        public ReactiveCommand<Unit, Unit> RefreshCommand { get; set; }
        public ReactiveCommand<Unit, Unit> PlayRandomCommand { get; set; }
        public ReactiveCommand<Unit, Unit> CacheAllCommand { get; set; }

        private TeensySettings _settings = null!;
        private readonly IMusicState _musicState;
        private readonly IDialogService _dialog;

        public MusicViewModel(IMusicState musicState, IGlobalState globalState, IDialogService dialog, ISettingsService settingsService, INavigationService nav, IAlertService alert, SongListViewModel songList, SearchMusicViewModel search)
        {
            FeatureTitle = "Music";
            _musicState = musicState;
            _dialog = dialog;
            SongList = songList;
            Search = search;
            PlayToolBar = new PlayToolbarViewModel
            (
                _musicState.CurrentSong,
                _musicState.LaunchState,
                _musicState.Time,
                _musicState.ToggleShuffleMode,
                _musicState.ToggleMusic,
                _musicState.PlayPrevious,
                _musicState.PlayNext,
                _musicState.SaveFavorite,
                _musicState.LoadDirectory,
                PlayToggleOption.Pause,
                alert
            );

            settingsService.Settings.Subscribe(s => _settings = s);

            musicState.CurrentSong
                .Select(s => s is null)
                .ToPropertyEx(this, x => x.ShowPlayToolbar);

            globalState.SerialConnected.ToPropertyEx(this, x => x.MusicAvailable);
            
            RefreshCommand = ReactiveCommand.CreateFromTask<Unit>(_ => musicState.RefreshDirectory());
            PlayRandomCommand = ReactiveCommand.CreateFromTask<Unit>(_ => musicState.PlayRandom());
            CacheAllCommand = ReactiveCommand.CreateFromTask(HandleCacheAll);

            MusicTree = new(musicState.DirectoryTree)
            {
                DirectorySelectedCommand = ReactiveCommand.CreateFromTask<DirectoryNodeViewModel>(async (directory) =>
                await musicState.LoadDirectory(directory.Path), outputScheduler: RxApp.MainThreadScheduler)
            };
        }

        private async Task HandleCacheAll()
        {
            var confirm = await _dialog.ShowConfirmation($"Cache All \r\rCreates a local index of all the files stored on your {_settings.TargetType} storage.  This will enable rich discovery of music and programs though search and file launch randomization.\r\rThis may take a few minutes if you have a lot of files from libraries like OneLoad64 or HSVC on your {_settings.TargetType} storage.\r\rIf you specified game art folders in your settings, it may take a few extra minutes to download those images from the TR.\r\rIt's worth the wait. Proceed?");

            if (!confirm) return;

            await _musicState.CacheAll();
        }
    }
}
