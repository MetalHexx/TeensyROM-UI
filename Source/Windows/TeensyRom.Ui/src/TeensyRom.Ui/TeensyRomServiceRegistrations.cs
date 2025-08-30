using Microsoft.Extensions.DependencyInjection;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Settings;
using TeensyRom.Ui.Features.Terminal;
using TeensyRom.Ui.Features.Help;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Features.Settings;
using TeensyRom.Ui.Main;
using System.Windows.Threading;
using MediatR;
using TeensyRom.Core.Music.Sid;
using TeensyRom.Ui.Services;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Games;
using TeensyRom.Ui.Features.Discover;
using TeensyRom.Ui.Features.Discover.State;
using TeensyRom.Ui.Features.Terminal.SerialCommand;
using TeensyRom.Ui.Core.Progress;
using TeensyRom.Ui.Features.Discover.State.Player;
using TeensyRom.Core.Storage.Tools.D64Extraction;
using TeensyRom.Core.Storage.Tools.Zip;
using TeensyRom.Core.Midi;
using TeensyRom.Ui.Controls.Playlist;
using TeensyRom.Ui.Services.Process;
using TeensyRom.Core.Commands.MuteSidVoices;
using TeensyRom.Core.Commands.SetMusicSpeed;
using TeensyRom.Core.Commands.PlaySubtune;
using TeensyRom.Core.Storage;
using TeensyRom.Ui.Services.Logging;
using TeensyRom.Ui.Services.Settings;
using TeensyRom.Core.Serial.Commands.ToggleMusic;
using TeensyRom.Core.Serial.Commands.Behaviors;
using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Device;

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
            services.AddSingleton<IProgressService, ProgressService>();
            services.AddSingleton<ISnackbarService, SnackbarService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IProgressTimer, ProgressTimer>();
            services.AddSingleton<ILaunchHistory,  LaunchHistory>();
            services.AddSingleton<IObservableSerialPort, ObservableSerialPort>();
            services.AddSingleton<ISerialStateContext, SerialStateContext>();
            services.AddSingleton<ISerialFactory, SerialFactory>();
            services.AddSingleton<IStorageFactory, StorageFactory>();
            services.AddSingleton<ICartFinder, CartFinder>();
            services.AddSingleton<ICartTagger, CartTagger>();
            services.AddSingleton<IDeviceConnectionManager, DeviceConnectionManager>();
            services.AddSingleton<IGameMetadataService, GameMetadataService>();
            services.AddSingleton<ISidMetadataService, SidMetadataService>();
            services.AddSingleton<IMidiService, MidiService>();
            services.AddSingleton<ICachedStorageService, CachedStorageService>();
            services.AddSingleton<IStorageCache, StorageCache>();
            services.AddSingleton<IPlayerContext, PlayerContext>();
            services.AddSingleton<IDiscoveryTreeState, DiscoverTreeState>();
            services.AddSingleton<ISetupService, SetupService>();  
            services.AddSingleton<ID64Extractor, D64Extractor>();
            services.AddSingleton<IZipExtractor, ZipExtractor>();
            services.AddSingleton<IFwVersionChecker, FwVersionChecker>();
            services.AddSingleton<ICopyFileProcess, CopyFileProcess>();
            services.AddSingleton<IFavoriteFileProcess, FavoriteFileProcess>();
            services.AddSingleton<IUpsertFileProcess, UpsertFileProcess>();
            services.AddSingleton<ILaunchFileProcess, LaunchFileProcess>();
            services.AddSingleton<ICrossProcessService, CrossProcessService>();
            services.AddSingleton<INamedPipeServer, NamedPipeServer>();
            services.AddSingleton<INamedPipeClient, NamedPipeClient>();
            services.AddSingleton<ISyncService, SyncService>();
            services.AddSingleton<NavigationHostViewModel>();
            services.AddSingleton<TerminalViewModel>();
            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<HelpViewModel>();
            services.AddSingleton<DiscoverViewModel>();
            services.AddSingleton<PlaylistViewModel>();
            services.AddSingleton<IPlaylistDialogService, PlaylistDialogService>();
            services.AddSingleton<ISerialCommandViewModel, SerialCommandViewModel>();
            services.AddSingleton<IMuteSidVoicesSerialRoutine, MuteSidVoicesSerialRoutine>();
            services.AddSingleton<IToggleMusicSerialRoutine, ToggleMusicSerialRoutine>();
            services.AddSingleton<ISetMusicSpeedSerialRoutine,  SetMusicSpeedSerialRoutine>();
            services.AddSingleton<IPlaySubtuneSerialRoutine, PlaySubtuneSerialRoutine>();
            services.AddSingleton<IFileTransferService,  FileTransferService>();
            services.AddSingleton<MainWindow>();
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CoreSerialAssemblyMarker>());            
            services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(ExceptionBehavior<,>));
            services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(SerialBehavior<,>)); 
        }
    }
}