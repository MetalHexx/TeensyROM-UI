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
using TeensyRom.Ui.Features.Help;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Features.Settings;
using TeensyRom.Ui.Main;
using TeensyRom.Core;
using System.Windows;
using System.Windows.Threading;
using MediatR;
using TeensyRom.Core.Music.Sid;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Services;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Games;
using TeensyRom.Ui.Controls.DirectoryTree;
using TeensyRom.Ui.Controls.DirectoryChips;
using TeensyRom.Ui.Controls.Search;
using TeensyRom.Ui.Controls.DirectoryList;
using TeensyRom.Ui.Features.Common.State.Progress;
using TeensyRom.Ui.Features.Discover;
using TeensyRom.Ui.Features.Discover.State;
using TeensyRom.Core.Assets.Tools.Vice;
using TeensyRom.Ui.Features.Connect.SerialCommand;

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
            services.AddSingleton<IGameMetadataService, GameMetadataService>();
            services.AddSingleton<ISidMetadataService, SidMetadataService>();
            services.AddSingleton<ICachedStorageService, CachedStorageService>();
            services.AddSingleton<IDiscoverContext, DiscoverContext>();
            services.AddSingleton<IDiscoverViewConfig, DiscoverViewConfig>();
            services.AddSingleton<IDiscoveryTreeState, DiscoverTreeState>();
            services.AddSingleton<ISetupService, SetupService>();  
            services.AddSingleton<ID64Extractor, D64Extractor>();
            services.AddSingleton<NavigationHostViewModel>();
            services.AddSingleton<ConnectViewModel>();
            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<HelpViewModel>();
            services.AddSingleton<DiscoverViewModel>();
            services.AddSingleton<SerialCommandViewModel>();
            services.AddSingleton<MainWindow>();
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CoreAssemblyMarker>());            
            services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(ExceptionBehavior<,>));
            services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(SerialBehavior<,>)); 
        }
    }
}