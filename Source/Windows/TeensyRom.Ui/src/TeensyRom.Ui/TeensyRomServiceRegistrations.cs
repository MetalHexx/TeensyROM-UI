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
using TeensyRom.Ui.Features.FileTransfer;
using TeensyRom.Ui.Features.Help;
using TeensyRom.Ui.Features.Music.MusicTree;
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

namespace TeensyRom.Ui
{
    public static class TeensyRomServiceRegistrations
    {
        public static void ConfigureTeensyServices(this IServiceCollection services, Dispatcher dispatcher)
        {
            services.AddSingleton(dispatcher);
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<ISerialPortState, SerialPortState>();
            services.AddSingleton<ILoggingService, LoggingService>();
            services.AddSingleton<IObservableSerialPort, ObservableSerialPort>();
            services.AddSingleton<IFileWatchService, FileWatchService>();
            services.AddSingleton<IFileWatcher, FileWatcher>();
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<ISnackbarService, SnackbarService>();
            services.AddSingleton<ISongTimer, SongTimer>();
            services.AddSingleton<IMusicState, MusicState>();
            services.AddSingleton<IFileState, FileState>();
            services.AddSingleton<ISidMetadataService, SidMetadataService>();
            services.AddSingleton<IMusicStorageService, MusicStorageService>();
            services.AddSingleton<NavigationHostViewModel>();
            services.AddSingleton<ConnectViewModel>();
            services.AddSingleton<FileTransferViewModel>();
            services.AddSingleton<FilesViewModel>();
            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<HelpViewModel>();
            services.AddSingleton<MusicViewModel>();
            services.AddSingleton<PlayToolbarViewModel>();
            services.AddSingleton<SongListViewModel>();
            services.AddSingleton<MusicTreeViewModel>();
            services.AddSingleton<MainWindow>();
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CoreAssemblyMarker>());
            services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        }
    }
}