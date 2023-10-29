using FluentAssertions;
using System.IO.Ports;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.Services;
using TeensyRom.Ui.Features.Connect;

namespace TeensyRom.Tests.Integration
{
    [Collection("SerialPortTests")]
    public class SerialPortTests : IDisposable
    {
        private ConnectViewModel _viewModel;
        private ITeensyObservableSerialPort _serialPort;
        public SerialPortTests()
        {
            var logService = new LoggingService();
            _serialPort = new TeensyObservableSerialPort(logService);
            _viewModel = new ConnectViewModel(_serialPort, logService);
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
        public void Given_NotConnected_When_Connecting_Then_Returns_ConnectionLogs()
        {
            //Arrange            
            var actualSelectedPort = SerialPort.GetPortNames().First();
            var expectedConnectedLog = $"Successfully connected to {actualSelectedPort}";

            //Act
            _viewModel.SelectedPort = actualSelectedPort;
            _viewModel.ConnectCommand.Execute().Subscribe();
            Thread.Sleep(500);

            //Assert
            _viewModel.Logs.Should().Contain(expectedConnectedLog);
        }

        [Fact]
        public void Given_NotConnected_When_Connecting_And_ErrorOccurs_Then_Returns_ErrorLog()
        {
            //Arrange            
            var actualSelectedPort = SerialPort.GetPortNames().First();
            var expectedErrorLog = $"Failed to ensure the connection to Not a real port. Retrying in 5000 ms";

            //Act
            _viewModel.SelectedPort = "Not a real port";
            Assert.Throws<ArgumentException>(() => _viewModel.ConnectCommand.Execute().Subscribe());
            Thread.Sleep(500);

            //Assert
            _viewModel.Logs.Should().Contain(expectedErrorLog);
        }

        [Fact]
        public void Given_Connected_When_Disconnecting_Then_Returns_DisconnectionLogs()
        {
            //Arrange            
            var actualSelectedPort = SerialPort.GetPortNames().First();
            var expectedDisconnectedLog = $"Successfully disconnected from {actualSelectedPort}.";

            //Act
            _viewModel.SelectedPort = actualSelectedPort;
            _viewModel.ConnectCommand.Execute().Subscribe();
            Thread.Sleep(500);
            _viewModel.DisconnectCommand.Execute().Subscribe();
            Thread.Sleep(500);

            //Assert
            _viewModel.Logs.Should().Contain(expectedDisconnectedLog);
        }

        [Fact]
        public void Given_Connected_When_Pinged_RespondsWithSuccessLog()
        {
            //Arrange            
            var actualSelectedPort = SerialPort.GetPortNames().First();
            var expectedPingLog = $"Pinging device";
            var expectedPongLog = $"TeensyROM";

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
        public void Given_Connected_When_Disconnecting_And_Reconnecting_Then_Ping_RespondsWithSuccessLog()
        {
            //Arrange            
            var actualSelectedPort = SerialPort.GetPortNames().First();
            var expectedPingLog = $"Pinging device";
            var expectedPongLog = $"TeensyROM";

            //Act
            _viewModel.SelectedPort = actualSelectedPort;

            _viewModel.ConnectCommand.Execute().Subscribe();
            Thread.Sleep(500);
            _viewModel.DisconnectCommand.Execute().Subscribe();
            Thread.Sleep(500);
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
        public void Given_Connected_When_Disconnected_Then_ConnectionStatusIsFalse()
        {
            //Arrange            
            var actualSelectedPort = SerialPort.GetPortNames().First();

            //Act
            _viewModel.SelectedPort = actualSelectedPort;
            _viewModel.ConnectCommand.Execute().Subscribe();
            Thread.Sleep(500);
            _viewModel.DisconnectCommand.Execute().Subscribe();
            Thread.Sleep(500);

            //Assert
            _viewModel.IsConnected.Should().BeFalse();
        }

        [Fact]
        public void Given_Connected_When_Disconnected_Then_PollToReconnect()
        {
            //TODO: Mock the serial port to test this behavior.
        }


        [Fact]
        public void Given_Connected_When_Polling_And_PortIsConnectable_Successfully_Reconnect()
        {
            //TODO: Mock the serial port to test this behavior.
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


        [Fact]
        public void Given_Connected_When_ResetClicked_Then_LogsReset()
        {
            //Arrange
            var actualSelectedPort = SerialPort.GetPortNames().First();
            var expectedLog1 = "Resetting device";
            var expectedLog2 = "Reset cmd received";
            var expectedLog3 = "Loading IO handler: TeensyROM";
            var expectedLog4 = "Resetting C64";

            //Act
            _viewModel.SelectedPort = actualSelectedPort;
            _viewModel.ConnectCommand.Execute().Subscribe();
            Thread.Sleep(500);
            _viewModel.ResetCommand.Execute().Subscribe();
            Thread.Sleep(1000);

            //Assert
            _viewModel.Logs.Should().Contain(expectedLog1);
            _viewModel.Logs.Should().Contain(expectedLog2);
            _viewModel.Logs.Should().Contain(expectedLog3);
            _viewModel.Logs.Should().Contain(expectedLog4);
        }

        public void Dispose()
        {
            _serialPort?.Dispose();
        }
    }
}