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
using TeensyRom.Ui.Controls.CornerToolbar;
using TeensyRom.Ui.Controls.DirectoryChips;
using TeensyRom.Ui.Controls.DirectoryList;
using TeensyRom.Ui.Controls.DirectoryTree;
using TeensyRom.Ui.Controls.FeatureTitle;
using TeensyRom.Ui.Controls.FileInfo;
using TeensyRom.Ui.Controls.LibraryFilter;
using TeensyRom.Ui.Controls.PlayToolbar;
using TeensyRom.Ui.Controls.Search;
using TeensyRom.Ui.Controls.SearchResultsToolbar;
using TeensyRom.Ui.Features.Common.Config;
using TeensyRom.Ui.Features.Common.Models;
using TeensyRom.Ui.Features.Common.State;
using TeensyRom.Ui.Features.Common.State.Player;
using TeensyRom.Ui.Features.Common.State.Progress;
using TeensyRom.Ui.Features.Discover.State;
using TeensyRom.Ui.Services;

namespace TeensyRom.Ui.Features.Discover
{
    public class DiscoverViewModel : ReactiveObject
    {
        [ObservableAsProperty] public bool IsConnected { get; set; }
        [ObservableAsProperty] public bool PlayToolbarActive { get; set; }
        [ObservableAsProperty] public bool SearchActive { get; }

        [Reactive] public LibraryFilterViewModel Filter { get; set; } = null!;
        [Reactive] public DirectoryChipsViewModel DirectoryChips { get; set; } = null!;
        [Reactive] public SearchViewModel Search { get; set; }
        [Reactive] public DirectoryTreeViewModel DirectoryTree { get; set; }
        [Reactive] public DirectoryListViewModel DirectoryList { get; set; }
        [Reactive] public FileInfoViewModel FileInfo { get; set; }
        [Reactive] public PlayToolbarViewModel PlayToolbar { get; set; }
        [Reactive] public CornerToolbarViewModel CornerToolbar { get; set; } = null!;
        [Reactive] public FeatureTitleViewModel Title { get; set; }
        [Reactive] public SearchResultsToolbarViewModel SearchResultsToolbar { get; set; } = new();

        private TeensySettings _settings = null!;
        private readonly IExplorerViewConfig _viewConfig;

        public DiscoverViewModel(IDiscoverContext context, ISerialStateContext serial, IDialogService dialog, IAlertService alert, ISettingsService settingsService, IGameMetadataService metadata, IDiscoverViewConfig config, IProgressTimer? timer)
        {
            _viewConfig = config;
            Title = new FeatureTitleViewModel("Discover");
            FileInfo = new FileInfoViewModel(context, metadata);

            serial.CurrentState
                .Select(state => state is SerialBusyState or SerialConnectedState)
                .ToPropertyEx(this, x => x.IsConnected);

            context.LaunchedFile
                .Select(g => g is not null)
                .ToPropertyEx(this, x => x.PlayToolbarActive);

            context.CurrentState
                .Select(state => state is SearchState)
                .ToPropertyEx(this, x => x.SearchActive);

            var launchState = context.CurrentState
                .Select(state => GetPlayMode(state))
                .CombineLatest(context.PlayingState, (mode, state) => (mode, state))
                .Select(stateMode => new LaunchItemState { PlayState = stateMode.state, PlayMode = stateMode.mode });

            PlayToolbar = new PlayToolbarViewModel
            (
                context.LaunchedFile,
                launchState,
                timer,
                context.ToggleShuffleMode,
                context.TogglePlay,
                context.PlayPrevious,
                context.PlayNext,
                context.SaveFavorite,
                context.LoadDirectory,
                alert
            );

            DirectoryList = new DirectoryListViewModel
            (
                context.DirectoryContent,
                context.PagingEnabled,
                context.CurrentPage,
                context.TotalPages,
                context.PlayFile,
                context.SelectFile,
                context.SaveFavorite,
                context.StoreFiles,
                context.DeleteFile,
                context.LoadDirectory,
                context.NextPage,
                context.PreviousPage,
                context.SetPageSize,
                alert: alert,
                dialog: dialog
            );

            DirectoryTree = new(context.DirectoryTree)
            {
                DirectorySelectedCommand = ReactiveCommand.CreateFromTask<DirectoryNodeViewModel>(
                    execute: async (directory) => await context.LoadDirectory(directory.Path),
                    outputScheduler: RxApp.MainThreadScheduler)
            };

            var searchActive = context.CurrentState.Select(s => s is SearchState);

            Search = new(searchActive)
            {
                SearchCommand = ReactiveCommand.Create<string, Unit>(
                    execute: context.SearchFiles,
                    outputScheduler: RxApp.MainThreadScheduler),

                ClearSearchCommand = ReactiveCommand.CreateFromTask(
                    execute: () =>
                    {
                        Search!.SearchText = string.Empty;
                        return context.ClearSearch();
                    },
                    outputScheduler: RxApp.MainThreadScheduler)
            };

            settingsService.Settings.Subscribe(s =>
            {
                _settings = s;

                var libs = _settings.FileFilters.Where(lib => lib.Type is not TeensyFilterType.Hex);

                var selectedFilter = Filter?.SelectedLibrary;

                Filter = new LibraryFilterViewModel(libs, selectedFilter, 
                    filterFunc: (filter) => context.SwitchFilter(filter),
                    launchRandomFunc: () => context.PlayRandom());

                DirectoryChips = new DirectoryChipsViewModel
                (
                    path: context.CurrentPath,
                    basePath: StorageConstants.Remote_Path_Root,
                    onClick: async path => await context.LoadDirectory(path),
                    onCopy: () => alert.Publish("Path copied to clipboard")
                );

                CornerToolbar = new CornerToolbarViewModel
                (
                    context.CacheAll,                    
                    context.RefreshDirectory,
                    dialog,
                    _settings.TargetType
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
