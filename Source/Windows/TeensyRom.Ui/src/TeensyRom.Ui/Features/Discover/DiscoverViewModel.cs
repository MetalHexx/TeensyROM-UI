using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Games;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Core.Storage.Services;
using TeensyRom.Ui.Controls.CacheButton;
using TeensyRom.Ui.Controls.DirectoryChips;
using TeensyRom.Ui.Controls.DirectoryList;
using TeensyRom.Ui.Controls.DirectoryTree;
using TeensyRom.Ui.Controls.FeatureTitle;
using TeensyRom.Ui.Controls.FileInfo;
using TeensyRom.Ui.Controls.LibraryFilter;
using TeensyRom.Ui.Controls.PlayToolbar;
using TeensyRom.Ui.Controls.Search;
using TeensyRom.Ui.Controls.SearchResultsToolbar;
using TeensyRom.Ui.Controls.StorageSelector;
using TeensyRom.Ui.Features.Common.Config;
using TeensyRom.Ui.Features.Common.Models;
using TeensyRom.Ui.Features.Discover.State;
using TeensyRom.Ui.Features.Discover.State.Player;
using TeensyRom.Ui.Features.Discover.State.Progress;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Services;

namespace TeensyRom.Ui.Features.Discover
{
    public class DiscoverViewModel : ReactiveObject
    {
        [ObservableAsProperty] public bool IsConnected { get; set; }
        [ObservableAsProperty] public bool ConnectionLost { get; set; }
        [ObservableAsProperty] public bool Disconnected { get; set; }

        [ObservableAsProperty] public bool PlayToolbarActive { get; set; }
        [ObservableAsProperty] public bool SearchActive { get; }

        [Reactive] public LibraryFilterViewModel Filter { get; set; } = null!;
        [Reactive] public DirectoryChipsViewModel DirectoryChips { get; set; } = null!;
        [Reactive] public SearchViewModel Search { get; set; }
        [Reactive] public DirectoryTreeViewModel DirectoryTree { get; set; }
        [Reactive] public DirectoryListViewModel DirectoryList { get; set; }
        [Reactive] public FileInfoViewModel FileInfo { get; set; }
        [Reactive] public PlayToolbarViewModel PlayToolbar { get; set; }
        [Reactive] public CacheButtonViewModel CornerToolbar { get; set; } = null!;
        [Reactive] public FeatureTitleViewModel Title { get; set; }
        [Reactive] public SearchResultsToolbarViewModel SearchResultsToolbar { get; set; } = new();
        [Reactive] public StorageSelectorViewModel StorageSelector { get; set; }

        private TeensySettings _settings = null!;

        public DiscoverViewModel(IPlayerContext player, ISerialStateContext serial, IDialogService dialog, IAlertService alert, IProgressService progress, ISettingsService settingsService, INavigationService nav, IGameMetadataService metadata, IProgressTimer? timer)
        {
            Title = new FeatureTitleViewModel("Discover");
            FileInfo = new FileInfoViewModel(player, metadata);

            var launchedFile = player.LaunchedFile.ObserveOn(RxApp.MainThreadScheduler);
            var serialCurrentState = serial.CurrentState.ObserveOn(RxApp.MainThreadScheduler);
            var playerContextState = player.CurrentState.ObserveOn(RxApp.MainThreadScheduler);

            serialCurrentState
                .Select(state => state is SerialBusyState or SerialConnectedState)
                .ToPropertyEx(this, x => x.IsConnected);

            serialCurrentState
                .Select(state => (state is SerialStartState or SerialConnectableState))
                .ToPropertyEx(this, x => x.Disconnected);

            serialCurrentState
                .Select(state => state is SerialConnectionLostState)
                .ToPropertyEx(this, x => x.ConnectionLost);

            launchedFile
                .Select(g => g is not null)
                .ToPropertyEx(this, x => x.PlayToolbarActive);

            playerContextState
                .Select(state => state is SearchState)
                .ToPropertyEx(this, x => x.SearchActive);

            var launchState = playerContextState
                .Select(state => GetPlayMode(state))
                .CombineLatest(player.PlayingState, (mode, state) => (mode, state))
                .Select(stateMode => new LaunchItemState { PlayState = stateMode.state, PlayMode = stateMode.mode });

            PlayToolbar = new PlayToolbarViewModel
            (
                launchedFile.Where(f => f is not null).Select(f => f.File),
                launchState,
                settingsService.Settings.Select(s => s.PlayTimerEnabled),
                player.CurrentScope,
                player.CurrentScopePath,
                timer,                
                player.ToggleShuffleMode,
                player.TogglePlay,
                player.PlayPrevious,
                player.PlayNext,
                player.PlaySubtune,
                player.SaveFavorite,
                player.RemoveFavorite,
                player.LoadDirectory,
                player.SetScope,                
                alert
            );

            DirectoryList = new DirectoryListViewModel
            (
                player.DirectoryContent.ObserveOn(RxApp.MainThreadScheduler),
                player.PagingEnabled.ObserveOn(RxApp.MainThreadScheduler),
                player.CurrentPage.ObserveOn(RxApp.MainThreadScheduler),
                player.TotalPages.ObserveOn(RxApp.MainThreadScheduler),
                player.PlayFile,
                player.SelectFile,
                player.SaveFavorite,
                player.RemoveFavorite,
                player.StoreFiles,
                player.DeleteFile,
                player.LoadDirectory,
                player.NextPage,
                player.PreviousPage,
                player.SetPageSize,
                alert,
                dialog,
                progress
            );

            StorageSelector = new StorageSelectorViewModel(settingsService);

            DirectoryTree = new(player.DirectoryTree)
            {
                DirectorySelectedCommand = ReactiveCommand.CreateFromTask<DirectoryNodeViewModel>(
                    execute: async (directory) => await player.LoadDirectory(directory.Path),
                    outputScheduler: RxApp.MainThreadScheduler)
            };

            var searchActive = playerContextState.Select(s => s is SearchState);

            Search = new(searchActive)
            {
                SearchCommand = ReactiveCommand.Create<string, Unit>(
                    execute: player.SearchFiles,
                    outputScheduler: RxApp.MainThreadScheduler),

                ClearSearchCommand = ReactiveCommand.CreateFromTask(
                    execute: () =>
                    {
                        Search!.SearchText = string.Empty;
                        return player.ClearSearch();
                    },
                    outputScheduler: RxApp.MainThreadScheduler)
            };

            settingsService.Settings.ObserveOn(RxApp.MainThreadScheduler).Subscribe(s =>
            {
                _settings = s;

                var libs = _settings.FileFilters.Where(lib => lib.Type is not TeensyFilterType.Hex);

                var selectedFilter = Filter?.SelectedLibrary;

                Filter = new LibraryFilterViewModel(
                    libs, 
                    selectedFilter, 
                    player.LaunchedFile.Where(f => f is not null && f.Random).Select(_ => Unit.Default),
                    filterFunc: (filter) => player.SwitchFilter(filter),
                    launchRandomFunc: () => player.PlayRandom());

                DirectoryChips = new DirectoryChipsViewModel
                (
                    path: player.CurrentPath.ObserveOn(RxApp.MainThreadScheduler),
                    basePath: StorageConstants.Remote_Path_Root,
                    onClick: async path => await player.LoadDirectory(path),
                    onPin: path => 
                    {
                        player.SetScopePath(path);
                        alert.Publish($"{path} has been pinned for shuffle mode.");
                    }
                );

                CornerToolbar = new CacheButtonViewModel
                (
                    player.CacheAll,                                        
                    dialog,
                    _settings.StorageType, 
                    nav
                );
            });
        }

        private static PlayMode GetPlayMode(PlayerState state) => state switch
        {
            NormalPlayState => PlayMode.Normal,
            ShuffleState => PlayMode.Shuffle,
            _ => PlayMode.Normal
        };
    }
}
