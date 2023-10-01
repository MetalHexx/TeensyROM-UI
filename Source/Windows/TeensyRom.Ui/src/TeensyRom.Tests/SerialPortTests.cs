using FluentAssertions;
using System.IO.Ports;
using TeensyRom.Core.Serial;
using TeensyRom.Ui.Features.Connect;

namespace TeensyRom.Tests
{
    [Collection("SerialPortTests")]
    public class SerialPortTests: IDisposable
    {
        private ConnectViewModel _viewModel;
        private ObservableSerialPort _serialPort;
        public SerialPortTests()
        {
            _serialPort = new ObservableSerialPort();
            _viewModel = new ConnectViewModel(_serialPort);
        }
        [Fact]
        public void Given_PortsExist_Then_PortsShouldMatch()
        {
            //Arrange
            var actualSerialPorts = SerialPort.GetPortNames();

            //Assert
            _viewModel.Ports.Should().BeEquivalentTo(actualSerialPorts);
        }

        [Fact]
        public void Given_PortsExist_Then_FirstPortSelected()
        {
            //Arrange
            var actualSelectedPort = SerialPort.GetPortNames().First();

            //Assert
            _viewModel.SelectedPort.Should().BeEquivalentTo(actualSelectedPort);
        }

        [Fact]
        public void Given_NotConnected_Then_Returns_NotConnectedLog()
        {
            //Arrange
            var expectedMessage = $"Not connected.";

            //Assert
            _viewModel.Logs.Should().Contain(expectedMessage);
        }

        [Fact]
        public void Given_NotConnected_When_Connecting_Then_Returns_ConnectionLogs()
        {
            //Arrange            
            var actualSelectedPort = SerialPort.GetPortNames().First();
            var expectedConnectingLog = $"Connecting to {actualSelectedPort}.";
            var expectedConnectedLog = $"Connection to {actualSelectedPort} successful.";

            //Act
            _viewModel.SelectedPort = actualSelectedPort;
            _viewModel.ConnectCommand.Execute().Subscribe();
            Thread.Sleep(500);

            //Assert
            _viewModel.Logs.Should().Contain(expectedConnectingLog);
            _viewModel.Logs.Should().Contain(expectedConnectedLog);
        }

        [Fact]
        public void Given_NotConnected_When_Connecting_And_ErrorOccurs_Then_Returns_ErrorLog()
        {
            //Arrange            
            var actualSelectedPort = SerialPort.GetPortNames().First();
            var expectedErrorLog = $"Failed to open the serial port: The given port name (Not a real port) does not resolve to a valid serial port. (Parameter 'portName')";

            //Act
            _viewModel.SelectedPort = "Not a real port";
            _viewModel.ConnectCommand.Execute().Subscribe();
            Thread.Sleep(500);

            //Assert
            _viewModel.Logs.Should().Contain(expectedErrorLog);
        }

        [Fact]
        public void Given_Connected_When_Pinged_RespondsWithSuccessLog()
        {
            //Arrange            
            var actualSelectedPort = SerialPort.GetPortNames().First();
            var expectedPingLog = $"Pinging device/C64";
            var expectedPongLog = $"> TeensyROM";

            //Act
            _viewModel.SelectedPort = actualSelectedPort;
            _viewModel.ConnectCommand.Execute().Subscribe();
            Thread.Sleep(500);
            _viewModel.PingCommand.Execute().Subscribe();
            Thread.Sleep(500);

            _viewModel.Logs.Should().Contain(expectedPingLog);
            _viewModel.Logs.Should().Contain(expectedPongLog);
        }

        [Fact]
        public void Given_NotConnected_Then_ConnectionStatusIsFalse()
        {
            //Arrange            
            var actualSelectedPort = SerialPort.GetPortNames().First();

            //Assert
            _viewModel.IsConnected.Should().BeFalse();
        }

        [Fact]
        public void Given_Connected_Then_ConnectionStatusIsTrue()
        {
            //Arrange            
            var actualSelectedPort = SerialPort.GetPortNames().First();

            //Act
            _viewModel.SelectedPort = actualSelectedPort;
            _viewModel.ConnectCommand.Execute().Subscribe();
            Thread.Sleep(500);

            //Assert
            _viewModel.IsConnected.Should().BeTrue();
        }

        public void Dispose()
        {
            _serialPort?.Dispose();
        }
    }
}