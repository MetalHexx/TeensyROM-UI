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
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Controls.CornerToolbar;
using TeensyRom.Ui.Controls.DirectoryChips;
using TeensyRom.Ui.Controls.DirectoryList;
using TeensyRom.Ui.Controls.DirectoryTree;
using TeensyRom.Ui.Controls.FeatureTitle;
using TeensyRom.Ui.Controls.FileInfo;
using TeensyRom.Ui.Controls.PlayToolbar;
using TeensyRom.Ui.Controls.Search;
using TeensyRom.Ui.Controls.SearchResultsToolbar;
using TeensyRom.Ui.Features.Common.Config;
using TeensyRom.Ui.Features.Common.State;
using TeensyRom.Ui.Features.Common.State.Player;
using TeensyRom.Ui.Features.Common.State.Progress;
using TeensyRom.Ui.Features.Global;
using TeensyRom.Ui.Features.Music.State;
using TeensyRom.Ui.Services;

namespace TeensyRom.Ui.Controls.Explorer
{
    public class ExplorerViewModel : ReactiveObject
    {
        [ObservableAsProperty] public bool IsConnected { get; set; }
        [ObservableAsProperty] public bool PlayToolbarActive { get; set; }
        [ObservableAsProperty] public bool SearchActive { get; }

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

        public ExplorerViewModel(string viewName, IPlayerContext player, IGlobalState globalState, IDialogService dialog, IAlertService alert, ISettingsService settingsService, IGameMetadataService metadata, IExplorerViewConfig config, IProgressTimer? timer)
        {
            _viewConfig = config;
            Title = new FeatureTitleViewModel(viewName);
            FileInfo = new FileInfoViewModel(player, metadata);

            globalState.SerialConnected.ToPropertyEx(this, x => x.IsConnected);

            player.LaunchedFile
                .Select(g => g is not null)
                .ToPropertyEx(this, x => x.PlayToolbarActive);

            player.CurrentState
                .Select(state => state is SearchState)
                .ToPropertyEx(this, x => x.SearchActive);

            var launchState = player.CurrentState
                .Select(state => GetPlayMode(state))
                .CombineLatest(player.PlayingState, (mode, state) => (mode, state))
                .Select(stateMode => new LaunchItemState { PlayState = stateMode.state, PlayMode = stateMode.mode });

            PlayToolbar = new PlayToolbarViewModel
            (
                player.LaunchedFile,
                launchState,
                timer,
                player.ToggleShuffleMode,
                player.TogglePlay,
                player.PlayPrevious,
                player.PlayNext,
                player.SaveFavorite,
                player.LoadDirectory,
                alert
            );

            DirectoryList = new DirectoryListViewModel
            (
                player.DirectoryContent,
                player.PagingEnabled,
                player.CurrentPage,
                player.TotalPages,
                player.PlayFile,
                player.SelectFile,
                player.SaveFavorite,
                player.DeleteFile,
                player.LoadDirectory,
                player.NextPage,
                player.PreviousPage,
                player.SetPageSize,
                alert,
                dialog
            );

            DirectoryTree = new(player.DirectoryTree)
            {
                DirectorySelectedCommand = ReactiveCommand.CreateFromTask<DirectoryNodeViewModel>(
                    execute: async (directory) => await player.LoadDirectory(directory.Path),
                    outputScheduler: RxApp.MainThreadScheduler)
            };

            var searchActive = player.CurrentState.Select(s => s is SearchState);

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

            settingsService.Settings.Subscribe(s =>
            {
                _settings = s;
                var libPath = s.Libraries.FirstOrDefault(l => l.Type == _viewConfig.LibraryType)?.Path ?? "";

                DirectoryChips = new DirectoryChipsViewModel(
                    path: player.CurrentPath,
                    basePath: libPath,
                    onClick: async path => await player.LoadDirectory(path),
                    onCopy: () => alert.Publish("Path copied to clipboard"));

                CornerToolbar = new CornerToolbarViewModel
                (
                    player.CacheAll,
                    player.PlayRandom,
                    player.RefreshDirectory,
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
