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
using TeensyRom.Core.Settings;
using TeensyRom.Ui.Controls.DirectoryTree;
using TeensyRom.Ui.Controls.Paging;
using TeensyRom.Ui.Features.Files.State;
using TeensyRom.Ui.Features.Games.GameInfo;
using TeensyRom.Ui.Features.Games.GameList;
using TeensyRom.Ui.Features.Games.GameToolbar;
using TeensyRom.Ui.Features.Games.Search;
using TeensyRom.Ui.Features.Games.State.NewState;
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
        [ObservableAsProperty] public bool ShowTree { get; }

        [Reactive] public SearchGamesViewModel Search { get; set; }
        [Reactive] public DirectoryTreeViewModel GamesTree { get; set; }
        [Reactive] public GameListViewModel GameList { get; set; }
        [Reactive] public GameInfoViewModel GameInfo { get; set; }
        [Reactive] public GameToolbarViewModel GameToolBar { get; set; }
        [Reactive] public PagingViewModel Paging { get; set; }

        public ReactiveCommand<Unit, Unit> RefreshCommand { get; set; }
        public ReactiveCommand<Unit, Unit> PlayRandomCommand { get; set; }
        public ReactiveCommand<Unit, Unit> CacheAllCommand { get; set; }

        private TeensySettings _settings;
        private readonly IFilePlayer _gameState;
        private readonly IDialogService _dialog;

        public GamesViewModel(IFilePlayer gameState, IGlobalState globalState, IDialogService dialog, ISettingsService settingsService, GameToolbarViewModel toolbar, GameListViewModel gameList, GameInfoViewModel gameInfo)
        {
            FeatureTitle = "Games";
            _gameState = gameState;
            _dialog = dialog;
            GameToolBar = toolbar;
            GameList = gameList;            
            GameInfo = gameInfo;
            settingsService.Settings.Subscribe(s => _settings = s);

            gameState.CurrentState
                .Select(s => s is not SearchPlayState)
                .ToPropertyEx(this, x => x.ShowTree);

            gameState.RunningGame
                .Select(g => g is not null)
                .ToPropertyEx(this, x => x.ShowToolbar);

            globalState.SerialConnected.ToPropertyEx(this, x => x.GamesAvailable);
            gameState.PagingEnabled.ToPropertyEx(this, x => x.ShowPaging);

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
                .Select(s => s is SearchPlayState);

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
            var confirm = await _dialog.ShowConfirmation($"Cache All \r\rThis will read all the files on your {_settings.TargetType} and save them to a local cache. Doing this will enable rich discovery of music and programs as it index all your files for search, random play and shuffle features.\r\rThis may take a few minutes if you have a lot of files from libraries like OneLoad64 or HSVC on your {_settings.TargetType} storage.\r\rProceed?");

            if (!confirm) return;

            await _gameState.CacheAll();
        }
    }
}
