using CommonServiceLocator;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
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

namespace TeensyRom.Ui
{
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;

        public App()
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<ISerialPortState, SerialPortState>();
            services.AddSingleton<ILoggingService, LoggingService>();
            services.AddSingleton<IObservableSerialPort, ObservableSerialPort>();
            services.AddSingleton<IResetCommand, ResetCommand>();
            services.AddSingleton<ISaveFileCommand, SaveFileCommand>();
            services.AddSingleton<IPingCommand, PingCommand>();
            services.AddSingleton<ILaunchFileCommand, LaunchFileCommand>();
            services.AddSingleton<IFileWatchService, FileWatchService>();
            services.AddSingleton<IGetDirectoryCommand, GetDirectoryCommand>();
            services.AddSingleton<IToggleMusicCommand, ToggleMusicCommand>();
            services.AddSingleton<ICopyFileCommand, CopyFileCommand>();
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
            services.AddSingleton(Current.Dispatcher);
        }
        private void OnStartup(object sender, StartupEventArgs e)
        {
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            var mainViewModel = _serviceProvider.GetRequiredService<NavigationHostViewModel>();
            mainWindow.DataContext = mainViewModel;
            mainWindow.Show();
        }
    }
}
