using FluentAssertions;
using Newtonsoft.Json;
using System.IO.Ports;
using System.Windows.Threading;
using TeensyRom.Core.Storage;
using TeensyRom.Ui.Features.FileTransfer;
using TeensyRom.Ui.Features.Settings;
using NavigationService = TeensyRom.Ui.Features.NavigationHost.NavigationService;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Core.Storage.Services;
using TeensyRom.Core.Settings.Entities;
using TeensyRom.Core.Settings.Services;
using TeensyRom.Core.Serial.Services;
using TeensyRom.Core.Logging;
using TeensyRom.Ui.Features.NavigationHost;

namespace TeensyRom.Tests.Integration
{
    [Collection("SerialPortTests")]
    public class FileTransferTests : IDisposable
    {
        private FileTransferViewModel _fileTransferViewModel;
        private SettingsViewModel _settingsViewModel;
        private ITeensyObservableSerialPort _teensyPort;
        private IFileWatcher _fileWatcher;
        private ISettingsService _settingsService;
        private ITeensyFileService _fileService;

        private readonly string _settingsFileName = "Settings.json";
        private readonly string _testFileName = $"{Guid.NewGuid().ToString().Substring(0, 7)}-test";
        private readonly string _fullSourceTestPath = string.Empty;

        private readonly string _serialPortName = SerialPort.GetPortNames().First();

        private readonly TeensySettings _settings;


        public FileTransferTests()
        {
            _settings = new TeensySettings();
            _fullSourceTestPath = @$"{_settings.WatchDirectoryLocation}\{_testFileName}";
        }

        [Fact]
        public void Given_WatcherDisabled_When_FileCopied_Then_FileNotTransferred()
        {
            //Arrange
            var fileDetectedText = @$"File detected: {_settings.WatchDirectoryLocation}\{_testFileName}.sid";
            var savedText = $"File transfer complete!";

            _settings.AutoFileCopyEnabled = false;
            InitializeViewModel();

            //Act
            File.WriteAllText($"{_fullSourceTestPath}.sid", "Test sid");
            Thread.Sleep(1000);

            //Assert
            _fileTransferViewModel.Logs.Should().NotContain(savedText);
        }

        [Fact]
        public void Given_WatcherEnabled_When_Disabled_And_Reenabled_And_FileCopied_Then_FileSuccessfullyTransferred()
        {
            //Arrange
            var fileDetectedText = @$"File detected: {_settings.WatchDirectoryLocation}\{_testFileName}.sid";
            var initiatedText = $"Initiating file transfer handshake";
            var savedText = $"File transfer complete!";
            var expectedType = $"Type: Sid";
            var storageType = $"Storage Type: SD";

            _settings.TargetType = TeensyStorageType.SD;
            _settings.TargetRootPath = TestConstants.Integration_Test_Root_Path; ;
            _settings.AutoFileCopyEnabled = true;
            InitializeViewModel();

            //Act            
            _settingsViewModel.Settings.AutoFileCopyEnabled = false;
            _settingsViewModel.SaveSettingsCommand.Execute().Subscribe();
            Thread.Sleep(500);
            _settingsViewModel.Settings.AutoFileCopyEnabled = true;
            _settingsViewModel.SaveSettingsCommand.Execute().Subscribe();

            File.WriteAllText($"{_fullSourceTestPath}.sid", "Test sid");
            Thread.Sleep(1000);

            //Assert
            _fileTransferViewModel.Logs.Should().Contain(savedText);
        }

        [Fact]
        public void Given_BadFilePath_When_FileSaved_Then_ReturnsError()
        {
            //Arrange
            var errorText = $"Failed to ensure directory";

            _settings.TargetType = TeensyStorageType.SD;
            _settings.TargetRootPath = "/$^*&#)@--bad/$^*&#)@--path/";
            _settings.AutoFileCopyEnabled = true;
            InitializeViewModel();

            //Act
            File.WriteAllText($"{_fullSourceTestPath}.sid", "Test sid");
            Thread.Sleep(1000);

            //Assert
            _fileTransferViewModel.Logs.Should().Contain(errorText);
        }

        [Fact]
        public void Given_EmptyRootFilePath_And_EmptyFileTargetPath_When_FileSaved_Then_ReturnsSuccess()
        {
            //Arrange
            var savedText = $"File transfer complete!"; ;

            _settings.TargetType = TeensyStorageType.SD;
            _settings.AutoFileCopyEnabled = true;
            InitializeViewModel();

            //Act
            _settingsViewModel.Settings.TargetRootPath = string.Empty;

            foreach (var target in _settingsViewModel.Settings.FileTargets)
            {
                target.TargetPath = string.Empty;
            }
            _settingsViewModel.SaveSettingsCommand.Execute().Subscribe();
            Thread.Sleep(500);

            //Act
            File.WriteAllText($"{_fullSourceTestPath}.sid", "Test sid");
            Thread.Sleep(1000);

            //Assert
            _fileTransferViewModel.Logs.Should().Contain(savedText);
        }

        [Fact]
        public void Given_NonExistantFileTargetPath_When_FileSaved_Then_ReturnsSuccess()
        {
            //Arrange
            var savedText = $"File transfer complete!"; ;

            _settings.TargetType = TeensyStorageType.SD;
            _settings.AutoFileCopyEnabled = true;
            _settings.TargetRootPath = TestConstants.Integration_Test_Root_Path; ;
            InitializeViewModel();

            //Act            
            foreach (var target in _settingsViewModel.Settings.FileTargets)
            {
                target.TargetPath = $"{Guid.NewGuid().ToString().Substring(0, 7)}-new-path";
            }
            _settingsViewModel.SaveSettingsCommand.Execute().Subscribe();
            Thread.Sleep(500);

            File.WriteAllText($"{_fullSourceTestPath}.sid", "Test sid");
            Thread.Sleep(1000);

            //Assert
            _fileTransferViewModel.Logs.Should().Contain(savedText);
        }

        [Fact]
        public void Given_EmptyRootFilePath_When_FileSaved_Then_ReturnsSuccess()
        {
            //Arrange
            var savedText = $"File transfer complete!"; ;

            _settings.TargetType = TeensyStorageType.SD;
            _settings.AutoFileCopyEnabled = true;
            InitializeViewModel();

            //Act
            _settingsViewModel.Settings.TargetRootPath = string.Empty;

            _settingsViewModel.SaveSettingsCommand.Execute().Subscribe();
            Thread.Sleep(500);

            File.WriteAllText($"{_fullSourceTestPath}.sid", "Test sid");
            Thread.Sleep(1000);

            //Assert
            _fileTransferViewModel.Logs.Should().Contain(savedText);
        }

        [Fact]
        public void Given_RootAsRoothPath_When_FileSaved_Then_ReturnsSuccess()
        {
            //Arrange
            var savedText = $"File transfer complete!"; ;

            _settings.TargetType = TeensyStorageType.SD;
            _settings.TargetRootPath = "/";
            _settings.AutoFileCopyEnabled = true;
            InitializeViewModel();

            //Act
            File.WriteAllText($"{_fullSourceTestPath}.sid", "Test sid");
            Thread.Sleep(1000);

            //Assert
            _fileTransferViewModel.Logs.Should().Contain(savedText);
        }

        [Fact]
        public void Given_WatcherDetectsNewFile_When_SidSaved_ToSD_Then_ReturnsSuccess()
        {
            //Arrange
            var fileDetectedText = @$"File detected: {_settings.WatchDirectoryLocation}\{_testFileName}.sid";
            var initiatedText = $"Initiating file transfer handshake";
            var savedText = $"File transfer complete!";
            var expectedType = $"Type: Sid";
            var storageType = $"Storage Type: SD";

            _settings.TargetType = TeensyStorageType.SD;
            _settings.TargetRootPath = TestConstants.Integration_Test_Root_Path; ;
            _settings.AutoFileCopyEnabled = true;
            InitializeViewModel();

            //Act
            File.WriteAllText($"{_fullSourceTestPath}.sid", "Test sid");
            Thread.Sleep(1000);

            //Assert
            _fileTransferViewModel.Logs.Should().Contain(fileDetectedText);
            _fileTransferViewModel.Logs.Should().Contain(initiatedText);
            _fileTransferViewModel.Logs.Should().Contain(savedText);
            _fileTransferViewModel.Logs.Should().Contain(expectedType);
            _fileTransferViewModel.Logs.Should().Contain(storageType);
        }

        [Fact]
        public void Given_WatcherDetectsNewFile_When_PrgSaved_ToSD_Then_ReturnsSuccess()
        {
            //Arrange
            var fileDetectedText = @$"File detected: {_settings.WatchDirectoryLocation}\{_testFileName}.prg";
            var initiatedText = $"Initiating file transfer handshake";
            var savedText = $"File transfer complete!";
            var expectedType = $"Type: Prg";
            var storageType = $"Storage Type: SD";

            _settings.TargetType = TeensyStorageType.SD;
            _settings.TargetRootPath = TestConstants.Integration_Test_Root_Path; ;
            _settings.AutoFileCopyEnabled = true;
            InitializeViewModel();

            //Act
            File.WriteAllText($"{_fullSourceTestPath}.prg", "Test prg");
            Thread.Sleep(1000);

            //Assert
            _fileTransferViewModel.Logs.Should().Contain(fileDetectedText);
            _fileTransferViewModel.Logs.Should().Contain(initiatedText);
            _fileTransferViewModel.Logs.Should().Contain(savedText);
            _fileTransferViewModel.Logs.Should().Contain(expectedType);
            _fileTransferViewModel.Logs.Should().Contain(storageType);
        }

        [Fact]
        public void Given_WatcherDetectsNewFile_When_CrtSaved_ToSD_Then_ReturnsSuccess()
        {
            //Arrange
            var fileDetectedText = @$"File detected: {_settings.WatchDirectoryLocation}\{_testFileName}.crt";
            var initiatedText = $"Initiating file transfer handshake";
            var savedText = $"File transfer complete!";
            var expectedType = $"Type: Crt";
            var storageType = $"Storage Type: SD";

            _settings.TargetType = TeensyStorageType.SD;
            _settings.TargetRootPath = TestConstants.Integration_Test_Root_Path; ;
            _settings.AutoFileCopyEnabled = true;
            InitializeViewModel();

            //Act            
            File.WriteAllText($"{_fullSourceTestPath}.crt", "Test crt");
            Thread.Sleep(1000);

            //Assert
            _fileTransferViewModel.Logs.Should().Contain(fileDetectedText);
            _fileTransferViewModel.Logs.Should().Contain(initiatedText);
            _fileTransferViewModel.Logs.Should().Contain(savedText);
            _fileTransferViewModel.Logs.Should().Contain(expectedType);
            _fileTransferViewModel.Logs.Should().Contain(storageType);
        }

        [Fact]
        public void Given_WatcherDetectsNewFile_When_HexSaved_ToSD_Then_ReturnsSuccess()
        {
            //Arrange
            var fileDetectedText = @$"File detected: {_settings.WatchDirectoryLocation}\{_testFileName}.hex";
            var initiatedText = $"Initiating file transfer handshake";
            var savedText = $"File transfer complete!";
            var expectedType = $"Type: Hex";
            var storageType = $"Storage Type: SD";

            _settings.TargetType = TeensyStorageType.SD;
            _settings.TargetRootPath = TestConstants.Integration_Test_Root_Path; ;
            _settings.AutoFileCopyEnabled = true;
            InitializeViewModel();

            //Act
            File.WriteAllText($"{_fullSourceTestPath}.hex", "Test hex");
            Thread.Sleep(1000);

            //Assert
            _fileTransferViewModel.Logs.Should().Contain(fileDetectedText);
            _fileTransferViewModel.Logs.Should().Contain(initiatedText);
            _fileTransferViewModel.Logs.Should().Contain(savedText);
            _fileTransferViewModel.Logs.Should().Contain(expectedType);
            _fileTransferViewModel.Logs.Should().Contain(storageType);
        }

        [Fact]
        public void Given_WatcherDetectsNewFile_When_SidSaved_ToUSB_Then_ReturnsSuccess()
        {
            //Arrange
            var fileDetectedText = @$"File detected: {_settings.WatchDirectoryLocation}\{_testFileName}.sid";
            var initiatedText = $"Initiating file transfer handshake";
            var savedText = $"File transfer complete!";
            var expectedType = $"Type: Sid";
            var storageType = $"Storage Type: USB";

            _settings.TargetType = TeensyStorageType.USB;
            _settings.TargetRootPath = TestConstants.Integration_Test_Root_Path; ;
            _settings.AutoFileCopyEnabled = true;
            InitializeViewModel();

            //Act
            File.WriteAllText($"{_fullSourceTestPath}.sid", "Test sid");
            Thread.Sleep(1000);

            //Assert
            _fileTransferViewModel.Logs.Should().Contain(fileDetectedText);
            _fileTransferViewModel.Logs.Should().Contain(initiatedText);
            _fileTransferViewModel.Logs.Should().Contain(savedText);
            _fileTransferViewModel.Logs.Should().Contain(expectedType);
            _fileTransferViewModel.Logs.Should().Contain(storageType);
        }

        [Fact]
        public void Given_WatcherDetectsNewFile_When_PrgSaved_ToUSB_Then_ReturnsSuccess()
        {
            //Arrange
            var fileDetectedText = @$"File detected: {_settings.WatchDirectoryLocation}\{_testFileName}.prg";
            var initiatedText = $"Initiating file transfer handshake";
            var savedText = $"File transfer complete!";
            var expectedType = $"Type: Prg";
            var storageType = $"Storage Type: USB";

            _settings.TargetType = TeensyStorageType.USB;
            _settings.TargetRootPath = TestConstants.Integration_Test_Root_Path; ;
            _settings.AutoFileCopyEnabled = true;
            InitializeViewModel();

            //Act
            File.WriteAllText($"{_fullSourceTestPath}.prg", "Test prg");
            Thread.Sleep(1000);

            //Assert
            _fileTransferViewModel.Logs.Should().Contain(fileDetectedText);
            _fileTransferViewModel.Logs.Should().Contain(initiatedText);
            _fileTransferViewModel.Logs.Should().Contain(savedText);
            _fileTransferViewModel.Logs.Should().Contain(expectedType);
            _fileTransferViewModel.Logs.Should().Contain(storageType);
        }

        [Fact]
        public void Given_WatcherDetectsNewFile_When_CrtSaved_ToUSB_Then_ReturnsSuccess()
        {
            //Arrange
            var fileDetectedText = @$"File detected: {_settings.WatchDirectoryLocation}\{_testFileName}.crt";
            var initiatedText = $"Initiating file transfer handshake";
            var savedText = $"File transfer complete!";
            var expectedType = $"Type: Crt";
            var storageType = $"Storage Type: USB";

            _settings.TargetType = TeensyStorageType.USB;
            _settings.TargetRootPath = TestConstants.Integration_Test_Root_Path; ;
            _settings.AutoFileCopyEnabled = true;
            InitializeViewModel();

            //Act
            File.WriteAllText($"{_fullSourceTestPath}.crt", "Test crt");
            Thread.Sleep(1000);

            //Assert
            _fileTransferViewModel.Logs.Should().Contain(fileDetectedText);
            _fileTransferViewModel.Logs.Should().Contain(initiatedText);
            _fileTransferViewModel.Logs.Should().Contain(savedText);
            _fileTransferViewModel.Logs.Should().Contain(expectedType);
            _fileTransferViewModel.Logs.Should().Contain(storageType);
        }

        [Fact]
        public void Given_WatcherDetectsNewFile_When_HexSaved_ToUSB_Then_ReturnsSuccess()
        {
            //Arrange
            var fileDetectedText = @$"File detected: {_settings.WatchDirectoryLocation}\{_testFileName}.hex";
            var initiatedText = $"Initiating file transfer handshake";
            var savedText = $"File transfer complete!";
            var expectedType = $"Type: Hex";
            var storageType = $"Storage Type: USB";

            _settings.TargetType = TeensyStorageType.USB;
            _settings.TargetRootPath = TestConstants.Integration_Test_Root_Path; ;
            _settings.AutoFileCopyEnabled = true;
            InitializeViewModel();

            //Act
            File.WriteAllText($"{_fullSourceTestPath}.hex", "Test hex");
            Thread.Sleep(1000);

            //Assert
            _fileTransferViewModel.Logs.Should().Contain(fileDetectedText);
            _fileTransferViewModel.Logs.Should().Contain(initiatedText);
            _fileTransferViewModel.Logs.Should().Contain(savedText);
            _fileTransferViewModel.Logs.Should().Contain(expectedType);
            _fileTransferViewModel.Logs.Should().Contain(storageType);
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
            _fileService = new TeensyFileService(_settingsService, _fileWatcher, _teensyPort, logService);
            var directoryService = new TeensyDirectoryService(_teensyPort, _settingsService, logService);
            var snackbar = new SnackbarService(Dispatcher.CurrentDispatcher);
            _fileTransferViewModel = new FileTransferViewModel(directoryService, _settingsService, _teensyPort, new NavigationService(), logService, snackbar, Dispatcher.CurrentDispatcher);            
            _settingsViewModel = new SettingsViewModel(_settingsService, snackbar, logService);
            _teensyPort.SetPort(_serialPortName);
            _teensyPort.OpenPort();
        }

        public void Dispose()
        {
            _teensyPort?.Dispose();
            _fileWatcher.Dispose();
            _fileService.Dispose();

            if (File.Exists(_fullSourceTestPath))
            {
                File.Delete(_fullSourceTestPath);
            }

            if (File.Exists(_settingsFileName))
            {
                File.Delete(_settingsFileName);
            }
        }
    }
}
