using FluentAssertions;
using Newtonsoft.Json;
using System.IO.Ports;
using System.Windows.Threading;
using TeensyRom.Ui.Features.FileTransfer;
using TeensyRom.Ui.Features.Settings;
using NavigationService = TeensyRom.Ui.Features.NavigationHost.NavigationService;
using TeensyRom.Core.Storage.Services;
using TeensyRom.Core.Settings.Entities;
using TeensyRom.Core.Settings.Services;
using TeensyRom.Core.Serial.Services;
using TeensyRom.Core.Logging;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Features.Connect;

namespace TeensyRom.Tests.Integration
{
    [Collection("SerialPortTests")]
    public class DirectoryTests : IDisposable
    {
        private ConnectViewModel _connectViewModel;
        private FileTransferViewModel _fileTransferViewModel;
        private SettingsViewModel _settingsViewModel;
        private ITeensyObservableSerialPort _teensyPort;
        private IFileWatcher _fileWatcher;
        private ISettingsService _settingsService;
        private ITeensyFileService _fileService;
        private readonly string _serialPortName = SerialPort.GetPortNames().First();

        private readonly string _settingsFileName = "Settings.json";

        private readonly TeensySettings _settings;

        public DirectoryTests()
        {
            _settings = new TeensySettings();
        }

        [Fact]
        public void POC_Given_Path_When_GetDirectoryListingCalled_ReturnsListing()
        {
            //Arrange 
            InitializeViewModel();            

            //Act
            _fileTransferViewModel.TestDirectoryListCommand.Execute().Subscribe();

            //Assert
            _fileTransferViewModel.Logs.Should().NotBeEmpty();

        }

        [Fact]
        public void Given_DefaultRootPathExists_When_FileTransferViewLoads_Then_CurrentDirectoryIsDefault()
        {
            //Arrange
            InitializeViewModel();
            //Assert
            _fileTransferViewModel.CurrentDirectory.Path.Should().Be("/sync/");
        }

        [Fact]
        public void Given_DirectoriesLoaded_When_DirectoryClicked_Then_CurrentDirectoryChangesToNewPath()
        {
            //Arrange
            _settings.TargetRootPath = TestConstants.Integration_Test_Root_Path;
            InitializeViewModel();            
            var directoryToSwitchTo = new DirectoryItemVm { Path = TestConstants.Integration_Test_Existing_Folder };

            //Act
            _fileTransferViewModel.LoadDirectoryContentCommand.Execute(directoryToSwitchTo).Subscribe();

            //Assert
            _fileTransferViewModel.CurrentDirectory!.Path.Should().Be(TestConstants.Integration_Test_Existing_Folder);
        }

        private void InitializeViewModel()
        {
            _settings.InitializeDefaults();

            var json = JsonConvert.SerializeObject(_settings);
            File.WriteAllText(_settingsFileName, json);

            var logService = new LoggingService(); 
            _teensyPort = new TeensyObservableSerialPort(logService);
            _fileWatcher = new FileWatcher();
            _settingsService = new SettingsService(logService);
            var directoryService = new TeensyDirectoryService(_teensyPort, _settingsService, logService);
            var navigationService = new NavigationService();
            var snackbar = new SnackbarService(Dispatcher.CurrentDispatcher);
            _fileTransferViewModel = new FileTransferViewModel(directoryService, _settingsService, _teensyPort, navigationService, logService, snackbar);            
            _settingsViewModel = new SettingsViewModel(_settingsService, snackbar, logService);
            _connectViewModel = new ConnectViewModel(_teensyPort, logService);
            _teensyPort.SetPort(_serialPortName);
            _teensyPort.OpenPort();
        }

        public void Dispose()
        {
            _teensyPort?.Dispose();
            _fileWatcher?.Dispose();
            _fileService?.Dispose();

            if (File.Exists(_settingsFileName))
            {
                File.Delete(_settingsFileName);
            }
        }
    }
}
