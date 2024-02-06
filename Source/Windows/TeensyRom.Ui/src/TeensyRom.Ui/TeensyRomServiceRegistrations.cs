using Microsoft.Extensions.DependencyInjection;
using TeensyRom.Core.Commands.File.LaunchFile;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Music;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage;
using TeensyRom.Core.Storage.Services;
using TeensyRom.Ui.Features.Connect;
using TeensyRom.Ui.Features.Files.State;
using TeensyRom.Ui.Features.Files;
using TeensyRom.Ui.Features.Help;
using TeensyRom.Ui.Features.Music.PlayToolbar;
using TeensyRom.Ui.Features.Music.SongList;
using TeensyRom.Ui.Features.Music.State;
using TeensyRom.Ui.Features.Music;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Features.Settings;
using TeensyRom.Ui.Main;
using TeensyRom.Core;
using System.Windows;
using System.Windows.Threading;
using MediatR;
using TeensyRom.Core.Music.Sid;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Features.Files.DirectoryContent;
using TeensyRom.Core.Commands.Behaviors;
using TeensyRom.Ui.Features.Music.Search;
using TeensyRom.Ui.Features.Files.Search;
using TeensyRom.Ui.Services;
using TeensyRom.Ui.Features.Global;
using TeensyRom.Core.Serial.State;
using TeensyRom.Ui.Features.Games;
using TeensyRom.Ui.Features.Games.GameToolbar;
using TeensyRom.Ui.Features.Games.GameList;
using TeensyRom.Ui.Features.Games.Search;
using TeensyRom.Ui.Features.Games.State;
using TeensyRom.Ui.Features.Games.GameInfo;
using TeensyRom.Core.Games;
using TeensyRom.Ui.Features.Games.State.NewState;
using TeensyRom.Ui.Controls.DirectoryTree;

namespace TeensyRom.Ui
{
    public static class TeensyRomServiceRegistrations
    {
        public static void ConfigureTeensyServices(this IServiceCollection services, Dispatcher dispatcher)
        {
            services.AddSingleton(dispatcher);
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<ILoggingService, LoggingService>();
            services.AddSingleton<IFileWatchService, FileWatchService>();
            services.AddSingleton<IFileWatcher, FileWatcher>();
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<IAlertService, AlertService>();
            services.AddSingleton<ISnackbarService, SnackbarService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<ISongTimer, SongTimer>();
            services.AddSingleton<ILaunchHistory,  LaunchHistory>();
            services.AddSingleton<IMusicState, MusicState>();
            services.AddSingleton<IPlayerContext, PlayerContext>();
            services.AddSingleton<IFileState, FileState>();
            services.AddSingleton<IGlobalState, GlobalState>();
            services.AddSingleton<IObservableSerialPort, ObservableSerialPort>();
            services.AddSingleton<ISerialStateContext, SerialStateContext>();
            services.AddSingleton<IGameMetadataService, GameMetadataService>();
            services.AddSingleton<ISidMetadataService, SidMetadataService>();
            services.AddSingleton<ICachedStorageService, CachedStorageService>();
            services.AddSingleton<IGameDirectoryTreeState, GameDirectoryState>();
            services.AddSingleton<NavigationHostViewModel>();
            services.AddSingleton<ConnectViewModel>();
            services.AddSingleton<FilesViewModel>();
            services.AddSingleton<SearchFilesViewModel>();
            services.AddSingleton<DirectoryContentViewModel>();
            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<HelpViewModel>();
            services.AddSingleton<MusicViewModel>();
            services.AddSingleton<GamesViewModel>();
            services.AddSingleton<PlayToolbarViewModel>();
            services.AddSingleton<SongListViewModel>();
            services.AddSingleton<SearchMusicViewModel>();
            services.AddSingleton<GamesViewModel>();
            services.AddSingleton<GameToolbarViewModel>();
            services.AddSingleton<GameListViewModel>();
            services.AddSingleton<GameInfoViewModel>();
            services.AddSingleton<SearchGamesViewModel>();
            services.AddSingleton<MainWindow>();
            services.AddSingleton<ITeensyCommandExecutor, TeensyCommandExecutor>();
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CoreAssemblyMarker>());            
            services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(ExceptionBehavior<,>));
            services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(SerialBehavior<,>)); 
        }
    }
}