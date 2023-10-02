using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using TeensyRom.Core.File;
using TeensyRom.Core.Serial;
using TeensyRom.Ui.Features.Connect;
using TeensyRom.Ui.Features.FileTransfer;

namespace TeensyRom.Tests
{
    [Collection("SerialPortTests")]
    public class FileTransferTests: IDisposable
    {
        private FileTransferViewModel _viewModel;
        private ITeensyObservableSerialPort _serialPort;
        private IFileWatcher _fileWatcher;
        private ITeensyFileService _fileService;
        private string _savePath = string.Empty;
        private string _testSidFilePath = string.Empty;
        private readonly string _serialPortName = string.Empty;

        public FileTransferTests()
        {
            _serialPort = new TeensyObservableSerialPort();
            _fileWatcher = new FileWatcher();
            _fileService = new TeensyFileService(_fileWatcher, _serialPort);
            _viewModel = new FileTransferViewModel(_fileService);

            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            _savePath = Path.Combine(userProfile, "Downloads");
            _testSidFilePath = Path.Combine(_savePath, $"{Guid.NewGuid()}-test-file.sid");
            _serialPortName = SerialPort.GetPortNames().First(); 
            _serialPort.SetPort(_serialPortName);
            _serialPort.OpenPort();
        }

        [Fact]
        public void Given_FileSaved_When_WatcherDetect_Then_ReturnsSaveLog()
        {
            //Act
            File.WriteAllText(_testSidFilePath, "Test sid");
            Thread.Sleep(5000);

            //Assert
            _viewModel.Logs.Should().Contain(_testSidFilePath);
        }

        public void Dispose()
        {
            _serialPort?.Dispose();
        }
    }
}
