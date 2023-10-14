using FluentAssertions;
using System.IO.Ports;
using TeensyRom.Core.Files;
using TeensyRom.Core.Files.Abstractions;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.Abstractions;
using TeensyRom.Core.Settings;
using TeensyRom.Ui.Features.FileTransfer;

namespace TeensyRom.Tests
{
    [Collection("SerialPortTests")]
    public class FileTransferTests: IDisposable
    {
        private FileTransferViewModel _viewModel;
        private ITeensyObservableSerialPort _teensyPort;
        private IFileWatcher _fileWatcher;
        private ISettingsService _settingsService;
        private ITeensyFileService _fileService;
        private string _savePath = string.Empty;
        private string _testSidFilePath = string.Empty;
        private readonly string _serialPortName = string.Empty;

        public FileTransferTests()
        {
            _teensyPort = new TeensyObservableSerialPort();
            _fileWatcher = new FileWatcher();
            _settingsService = new SettingsService();
            _fileService = new TeensyFileService(_settingsService, _fileWatcher, _teensyPort);
            _viewModel = new FileTransferViewModel(_fileService);

            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            _savePath = Path.Combine(userProfile, "Downloads");
            _testSidFilePath = Path.Combine(_savePath, $"{Guid.NewGuid().ToString().Substring(0, 7)}-test.sid");
            _serialPortName = SerialPort.GetPortNames().First(); 
            _teensyPort.SetPort(_serialPortName);
            _teensyPort.OpenPort();
        }

        [Fact]
        public void Given_FileSaved_When_WatcherDetectsFile_Then_ReturnsSaveLog()
        {
            //Arrange
            var fileDetectedText = @$"File detected: {_testSidFilePath}";
            var initiatedText = $"Initiating file transfer handshake";
            var savedText = $"File transfer complete!";
            //Act
            File.WriteAllText(_testSidFilePath, "Test sid");
            Thread.Sleep(1000);

            //Assert
            _viewModel.Logs.Should().Contain(fileDetectedText);
            _viewModel.Logs.Should().Contain(initiatedText);
            _viewModel.Logs.Should().Contain(savedText);
        }

        public void Dispose()
        {
            _teensyPort?.Dispose();
            _fileWatcher.Dispose();
            _fileService.Dispose();
            File.Delete(_testSidFilePath);
        }
    }
}
