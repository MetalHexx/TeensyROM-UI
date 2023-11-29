using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using MaterialDesignThemes.Wpf;
using System.Windows;
using TeensyRom.Core.Storage;
using TeensyRom.Ui.Features.Connect;
using TeensyRom.Ui.Features.FileTransfer;
using TeensyRom.Ui.Features.Help;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Features.Settings;
using TeensyRom.Ui.Features.Music;
using TeensyRom.Core.Storage.Services;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Commands.File.LaunchFile;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Settings;
using TeensyRom.Ui.Features.Music.PlayToolbar;
using TeensyRom.Ui.Features.Music.SongList;
using TeensyRom.Core.Music;
using TeensyRom.Ui.Features.Music.State;
using TeensyRom.Ui.Features.Music.MusicTree;

namespace TeensyRom.Ui
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Registers items for dependency injection.  
        /// <remarks>You can use this to inject classes into the constructors of your classes like view models</remarks>
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Register(() => Application.Current.Dispatcher);
            SimpleIoc.Default.Register<DialogHost, DialogHost>();
            SimpleIoc.Default.Register<ILoggingService, LoggingService>();
            SimpleIoc.Default.Register<INavigationService, NavigationService>();
            SimpleIoc.Default.Register<IObservableSerialPort, ObservableSerialPort>();
            SimpleIoc.Default.Register<ISerialPortState, SerialPortState>();
            SimpleIoc.Default.Register<IResetCommand, ResetCommand>();
            SimpleIoc.Default.Register<ISaveFileCommand, SaveFileCommand>();
            SimpleIoc.Default.Register<IPingCommand, PingCommand>();
            SimpleIoc.Default.Register<ILaunchFileCommand, LaunchFileCommand>();
            SimpleIoc.Default.Register<IFileWatchService, FileWatchService>();
            SimpleIoc.Default.Register<IGetDirectoryCommand, GetDirectoryCommand>();
            SimpleIoc.Default.Register<IToggleMusicCommand, ToggleMusicCommand>();
            SimpleIoc.Default.Register<ICopyFileCommand, CopyFileCommand>();
            SimpleIoc.Default.Register<IFileWatcher, FileWatcher>();
            SimpleIoc.Default.Register<ISettingsService, SettingsService>();
            SimpleIoc.Default.Register<ISnackbarService, SnackbarService>();
            SimpleIoc.Default.Register<ISongTimer, SongTimer>();
            SimpleIoc.Default.Register<IMusicState, MusicState>();
            SimpleIoc.Default.Register<ISidMetadataService, SidMetadataService>();
            SimpleIoc.Default.Register<IMusicStorageService, MusicStorageService>();
            SimpleIoc.Default.Register<NavigationHostViewModel>();
            SimpleIoc.Default.Register<ConnectViewModel>();
            SimpleIoc.Default.Register<FileTransferViewModel>();
            SimpleIoc.Default.Register<SettingsViewModel>();
            SimpleIoc.Default.Register<HelpViewModel>();
            SimpleIoc.Default.Register<MusicViewModel>();
            SimpleIoc.Default.Register<PlayToolbarViewModel>();
            SimpleIoc.Default.Register<SongListViewModel>();
            SimpleIoc.Default.Register<MusicTreeViewModel>();

            ServiceLocator.Current.GetInstance<IFileWatchService>();

        }

        /// <summary>
        /// Defaults the initial view to the Navigation Host View
        /// </summary>
        public NavigationHostViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<NavigationHostViewModel>();
            }
        }

        public static void Cleanup()
        {
            //TODO: Clear the ViewModels
        }
    }
}
