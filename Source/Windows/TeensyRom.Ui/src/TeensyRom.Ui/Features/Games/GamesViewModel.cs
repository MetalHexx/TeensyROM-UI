using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using TeensyRom.Core.Settings;
using TeensyRom.Ui.Controls.DirectoryTree;
using TeensyRom.Ui.Features.Games.GameInfo;
using TeensyRom.Ui.Features.Games.GameList;
using TeensyRom.Ui.Features.Games.GameToolbar;
using TeensyRom.Ui.Features.Games.Search;
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
       
        [Reactive] public SearchGamesViewModel Search { get; set; }
        [Reactive] public DirectoryTreeViewModel GamesTree { get; set; }
        [Reactive] public GameListViewModel GameList { get; set; }
        [Reactive] public GameInfoViewModel GameInfo { get; set; }
        [Reactive] public GameToolbarViewModel GameToolBar { get; set; }

        public ReactiveCommand<Unit, Unit> RefreshCommand { get; set; }
        public ReactiveCommand<Unit, Unit> PlayRandomCommand { get; set; }
        public ReactiveCommand<Unit, Unit> CacheAllCommand { get; set; }

        private TeensySettings _settings;
        private readonly IGameState _gameState;
        private readonly IDialogService _dialog;

        public GamesViewModel(IGameState gameState, IGlobalState globalState, IDialogService dialog, ISettingsService settingsService, GameToolbarViewModel toolbar, GameListViewModel gameList, SearchGamesViewModel search, GameInfoViewModel gameInfo)
        {
            FeatureTitle = "Games";
            _gameState = gameState;
            _dialog = dialog;
            GameToolBar = toolbar;
            GameList = gameList;
            Search = search;
            GameInfo = gameInfo;
            settingsService.Settings.Subscribe(s => _settings = s);

            gameState.RunningGame
                .Select(s => s is null)
                .ToPropertyEx(this, x => x.ShowToolbar);

            globalState.SerialConnected.ToPropertyEx(this, x => x.GamesAvailable);

            RefreshCommand = ReactiveCommand.CreateFromTask<Unit>(_ => gameState.RefreshDirectory());
            PlayRandomCommand = ReactiveCommand.CreateFromTask<Unit>(_ => gameState.PlayRandom());
            CacheAllCommand = ReactiveCommand.CreateFromTask(HandleCacheAll);

            GamesTree = new(gameState.DirectoryTree)
            {
                DirectorySelectedCommand = ReactiveCommand.CreateFromTask<DirectoryNodeViewModel>(async (directory) =>
                await gameState.LoadDirectory(directory.Path), outputScheduler: RxApp.MainThreadScheduler)
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
