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
using TeensyRom.Ui.Controls.DirectoryChips;
using TeensyRom.Ui.Controls.DirectoryList;
using TeensyRom.Ui.Controls.DirectoryTree;
using TeensyRom.Ui.Controls.Paging;
using TeensyRom.Ui.Controls.Search;
using TeensyRom.Ui.Features.Files.State;
using TeensyRom.Ui.Features.Games.GameInfo;
using TeensyRom.Ui.Features.Games.GameToolbar;
using TeensyRom.Ui.Features.Games.State;
using TeensyRom.Ui.Features.Global;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Helpers.ViewModel;
using TeensyRom.Ui.Services;

namespace TeensyRom.Ui.Features.Games
{
    public class GamesViewModel : FeatureViewModelBase
    {
        [ObservableAsProperty] public bool GamesAvailable { get; set; }
        [ObservableAsProperty] public bool ShowToolbar { get; set; }
        [ObservableAsProperty] public bool ShowPaging { get; }
        [ObservableAsProperty] public bool SearchActive { get; }

        [Reactive] public DirectoryChipsViewModel DirectoryChips { get; set; } = null!;
        [Reactive] public SearchViewModel Search { get; set; }
        [Reactive] public DirectoryTreeViewModel GamesTree { get; set; }
        [Reactive] public DirectoryListViewModel GameList { get; set; }
        [Reactive] public GameInfoViewModel GameInfo { get; set; }
        [Reactive] public GameToolbarViewModel GameToolBar { get; set; }
        [Reactive] public PagingViewModel Paging { get; set; }

        public ReactiveCommand<Unit, Unit> RefreshCommand { get; set; }
        public ReactiveCommand<Unit, Unit> PlayRandomCommand { get; set; }
        public ReactiveCommand<Unit, Unit> CacheAllCommand { get; set; }

        private TeensySettings _settings = null!;
        private readonly IPlayerContext _gameState;
        private readonly IDialogService _dialog;

        public GamesViewModel(IPlayerContext gameState, IGlobalState globalState, IDialogService dialog, IAlertService alert, ISettingsService settingsService, GameToolbarViewModel toolbar, GameInfoViewModel gameInfo)
        {
            FeatureTitle = "Games";
            _gameState = gameState;
            _dialog = dialog;
            GameToolBar = toolbar;
            GameInfo = gameInfo;
            GameList = new DirectoryListViewModel(gameState, alert, dialog);

            var gamesLibaryPath = settingsService.Settings.Select(s => s.Libraries.FirstOrDefault(l => l.Type == TeensyLibraryType.Programs)?.Path ?? "");
            var chipsObservable = gamesLibaryPath.CombineLatest(gameState.CurrentPath, (libraryPath, currentPath) => currentPath.Replace(libraryPath, ""));

            settingsService.Settings.Subscribe(s => 
            {
                _settings = s;
                var libPath = s.Libraries.FirstOrDefault(l => l.Type == TeensyLibraryType.Programs)?.Path ?? "";

                DirectoryChips = new DirectoryChipsViewModel(
                    path: gameState.CurrentPath,
                    basePath: libPath,
                    onClick: async path => await gameState.LoadDirectory(path),
                    onCopy: () => alert.Publish("Path copied to clipboard"));
            });

            globalState.SerialConnected.ToPropertyEx(this, x => x.GamesAvailable);
            gameState.PagingEnabled.ToPropertyEx(this, x => x.ShowPaging);

            gameState.LaunchedGame
                .Select(g => g is not null)
                .ToPropertyEx(this, x => x.ShowToolbar);

            gameState.CurrentState
                .Select(state => state is SearchState)
                .ToPropertyEx(this, x => x.SearchActive);

            RefreshCommand = ReactiveCommand.CreateFromTask<Unit>( 
                execute: _ => gameState.RefreshDirectory(), 
                outputScheduler: RxApp.MainThreadScheduler);

            PlayRandomCommand = ReactiveCommand.CreateFromTask<Unit>(
                execute: _ => gameState.PlayRandom(),
                outputScheduler: RxApp.MainThreadScheduler);

            CacheAllCommand = ReactiveCommand.CreateFromTask(
                execute: HandleCacheAll,
                outputScheduler: RxApp.MainThreadScheduler);

            GamesTree = new(gameState.DirectoryTree)
            {
                DirectorySelectedCommand = ReactiveCommand.CreateFromTask<DirectoryNodeViewModel>(
                    execute: async (directory) => await gameState.LoadDirectory(directory.Path), 
                    outputScheduler: RxApp.MainThreadScheduler)
            };

            Paging = new(gameState.CurrentPage, gameState.TotalPages)
            {
                NextPageCommand = ReactiveCommand.Create<Unit, Unit>(
                    execute: _ => gameState.NextPage(),
                    outputScheduler: RxApp.MainThreadScheduler),

                PreviousPageCommand = ReactiveCommand.Create<Unit, Unit>(
                    execute: _ => gameState.PreviousPage(),
                    outputScheduler: RxApp.MainThreadScheduler),

                PageSizeCommand = ReactiveCommand.Create<int, Unit>(
                    execute: gameState.SetPageSize,
                    outputScheduler: RxApp.MainThreadScheduler)
            };

            var searchEnabled = gameState.CurrentState
                .Select(s => s is SearchState);

            Search = new(searchEnabled)
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
        }

        private async Task HandleCacheAll()
        {
            var confirm = await _dialog.ShowConfirmation($"Cache All \r\rCreates a local index of all the files stored on your {_settings.TargetType} storage.  This will enable rich discovery of music and programs though search and file launch randomization.\r\rThis may take a few minutes if you have a lot of files from libraries like OneLoad64 or HSVC on your {_settings.TargetType} storage.\r\rIf you specified game art folders in your settings, it may take a few extra minutes to download those images from the TR.\r\rIt's worth the wait. Proceed?");

            if (!confirm) return;

            await _gameState.CacheAll();
        }
    }
}
