using FluentAssertions;
using Newtonsoft.Json;
using System.IO.Ports;
using System.Windows.Threading;
using TeensyRom.Core.Storage;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.Abstractions;
using TeensyRom.Core.Settings;
using TeensyRom.Ui.Features.FileTransfer;
using TeensyRom.Ui.Features.Settings;
using NavigationService = TeensyRom.Ui.Features.NavigationHost.NavigationService;
using TeensyRom.Core.Storage.Services;

namespace TeensyRom.Tests
{
    [Collection("SerialPortTests")]
    public class DirectoryTests : IDisposable
    {
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

        private void InitializeViewModel()
        {
            _settings.InitializeDefaults();

            var json = JsonConvert.SerializeObject(_settings);
            File.WriteAllText(_settingsFileName, json);

            _teensyPort = new TeensyObservableSerialPort();
            _fileWatcher = new FileWatcher();
            _settingsService = new SettingsService();
            _fileService = new TeensyFileService(_settingsService, _fileWatcher, _teensyPort);
            var navigationService = new NavigationService();
            _fileTransferViewModel = new FileTransferViewModel(_fileService, _settingsService, _teensyPort, navigationService);
            _settingsViewModel = new SettingsViewModel(_settingsService, Dispatcher.CurrentDispatcher);
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
