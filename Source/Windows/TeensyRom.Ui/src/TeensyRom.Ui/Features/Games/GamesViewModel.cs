using MaterialDesignColors;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using TeensyRom.Core.Common;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Settings;
using TeensyRom.Ui.Controls.CornerToolbar;
using TeensyRom.Ui.Controls.DirectoryChips;
using TeensyRom.Ui.Controls.DirectoryList;
using TeensyRom.Ui.Controls.DirectoryTree;
using TeensyRom.Ui.Controls.FeatureTitle;
using TeensyRom.Ui.Controls.Paging;
using TeensyRom.Ui.Controls.PlayToolbar;
using TeensyRom.Ui.Controls.Search;
using TeensyRom.Ui.Controls.SearchResultsToolbar;
using TeensyRom.Ui.Features.Common.State;
using TeensyRom.Ui.Features.Files.State;
using TeensyRom.Ui.Features.Games.GameInfo;
using TeensyRom.Ui.Features.Games.State;
using TeensyRom.Ui.Features.Global;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Helpers.ViewModel;
using TeensyRom.Ui.Services;

namespace TeensyRom.Ui.Features.Games
{
    public class GamesViewModel : FeatureViewModelBase
    {
        [ObservableAsProperty] public bool IsConnected { get; set; }
        [ObservableAsProperty] public bool PlayToolbarActive { get; set; }
        [ObservableAsProperty] public bool SearchActive { get; }

        [Reactive] public DirectoryChipsViewModel DirectoryChips { get; set; } = null!;
        [Reactive] public SearchViewModel Search { get; set; }
        [Reactive] public DirectoryTreeViewModel DirectoryTree { get; set; }
        [Reactive] public DirectoryListViewModel DirectoryList { get; set; }
        [Reactive] public GameInfoViewModel FileInfo { get; set; }
        [Reactive] public PlayToolbarViewModel PlayToolbar { get; set; }
        [Reactive] public CornerToolbarViewModel CornerToolbar { get; set; } = null!;
        [Reactive] public FeatureTitleViewModel Title { get; set; }
        [Reactive] public SearchResultsToolbarViewModel SearchResultsToolbar { get; set; } = new();

        private TeensySettings _settings = null!;

        public GamesViewModel(IPlayerContext gameState, IGlobalState globalState, IDialogService dialog, IAlertService alert, ISettingsService settingsService, GameInfoViewModel gameInfo)
        {   
            FileInfo = gameInfo;
            FeatureTitle = "Games";
            Title = new FeatureTitleViewModel(FeatureTitle);

            globalState.SerialConnected.ToPropertyEx(this, x => x.IsConnected);

            gameState.LaunchedGame
                .Select(g => g is not null)
                .ToPropertyEx(this, x => x.PlayToolbarActive);

            gameState.CurrentState
                .Select(state => state is SearchState)
                .ToPropertyEx(this, x => x.SearchActive);

            var launchState = gameState.CurrentState
                .Select(state => GetPlayMode(state))
                .CombineLatest(gameState.PlayingState, (mode, state) => (mode, state))
                .Select(stateMode => new LaunchItemState { PlayState = stateMode.state, PlayMode = stateMode.mode });

            PlayToolbar = new PlayToolbarViewModel
            (
                gameState.LaunchedGame,
                launchState,
                null,
                gameState.ToggleShuffleMode,
                gameState.ToggleGame,
                gameState.PlayPrevious,
                gameState.PlayNext,
                gameState.SaveFavorite,
                gameState.LoadDirectory,
                PlayToggleOption.Stop,
                alert
            );

            DirectoryList = new DirectoryListViewModel
            (
                gameState.DirectoryContent,
                gameState.PagingEnabled,
                gameState.CurrentPage,
                gameState.TotalPages,
                gameState.PlayGame, 
                gameState.SetSelectedGame, 
                gameState.SaveFavorite, 
                gameState.DeleteFile, 
                gameState.LoadDirectory,
                gameState.NextPage,
                gameState.PreviousPage,
                gameState.SetPageSize,
                alert, 
                dialog
            );

            DirectoryTree = new(gameState.DirectoryTree)
            {
                DirectorySelectedCommand = ReactiveCommand.CreateFromTask<DirectoryNodeViewModel>(
                    execute: async (directory) => await gameState.LoadDirectory(directory.Path),
                    outputScheduler: RxApp.MainThreadScheduler)
            };

            var searchActive = gameState.CurrentState.Select(s => s is SearchState);

            Search = new(searchActive)
            {
                SearchCommand = ReactiveCommand.Create<string, Unit>(
                    execute: gameState.SearchGames,
                    outputScheduler: RxApp.MainThreadScheduler),

                ClearSearchCommand = ReactiveCommand.CreateFromTask(
                    execute: () =>
                    {
                        Search!.SearchText = string.Empty;
                        return gameState.ClearSearch();
                    },
                    outputScheduler: RxApp.MainThreadScheduler)
            };

            settingsService.Settings.Subscribe(s => 
            {
                _settings = s;
                var libPath = s.Libraries.FirstOrDefault(l => l.Type == TeensyLibraryType.Programs)?.Path ?? "";

                DirectoryChips = new DirectoryChipsViewModel(
                    path: gameState.CurrentPath,
                    basePath: libPath,
                    onClick: async path => await gameState.LoadDirectory(path),
                    onCopy: () => alert.Publish("Path copied to clipboard"));

                CornerToolbar = new CornerToolbarViewModel
                (
                    gameState.CacheAll,
                    gameState.PlayRandom,
                    gameState.RefreshDirectory,
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