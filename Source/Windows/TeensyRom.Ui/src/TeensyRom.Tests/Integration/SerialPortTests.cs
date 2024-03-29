using FluentAssertions;
using System.IO.Ports;

namespace TeensyRom.Tests.Integration
{
    [Collection("SerialPortTests")]
    public class SerialPortTests : IDisposable
    {
        private readonly TeensyFixture _fixture;
        public SerialPortTests()
        {
            _fixture = new();
        }

        [Fact]
        public void Given_PortsExist_Then_PortsShouldMatch()
        {
            //Arrange
            var actualSerialPorts = SerialPort.GetPortNames();
            _fixture.Initialize(initOpenPort: false);

            //Assert
            _fixture.ConnectViewModel.Ports.Should().BeEquivalentTo(actualSerialPorts);
        }

        [Fact]
        public void Given_PortsExist_Then_FirstPortSelected()
        {
            //Arrange
            var actualSelectedPort = SerialPort.GetPortNames().First();
            _fixture.Initialize(initOpenPort: false);

            //Assert
            _fixture.ConnectViewModel.SelectedPort.Should().BeEquivalentTo(actualSelectedPort);
        }

        [Fact]
        public void Given_NotConnected_When_Connecting_Then_Returns_ConnectionLogs()
        {
            //Arrange            
            var actualSelectedPort = SerialPort.GetPortNames().First();
            var expectedConnectedLog = $"Successfully connected to {actualSelectedPort}";
            _fixture.Initialize(initOpenPort: true);

            //Act
            _fixture.ConnectViewModel.SelectedPort = actualSelectedPort;
            _fixture.ConnectViewModel.ConnectCommand.Execute().Subscribe();
            Thread.Sleep(500);

            //Assert
            _fixture.ConnectViewModel.Log.Logs.Should().Contain(expectedConnectedLog);
        }

        [Fact]
        public void Given_NotConnected_When_Connecting_And_ErrorOccurs_Then_Returns_ErrorLog()
        {
            //Arrange            
            var actualSelectedPort = SerialPort.GetPortNames().First();
            var expectedErrorLog = $"Failed to ensure the connection to Not a real port. Retrying in 5000 ms";
            _fixture.Initialize(initOpenPort: false);

            //Act
            _fixture.ConnectViewModel.SelectedPort = "Not a real port";
            Assert.Throws<ArgumentException>(() => _fixture.ConnectViewModel.ConnectCommand.Execute().Subscribe());
            Thread.Sleep(500);

            //Assert
            _fixture.ConnectViewModel.Log.Logs.Should().Contain(expectedErrorLog);
        }

        [Fact]
        public void Given_Connected_When_Disconnecting_Then_Returns_DisconnectionLogs()
        {
            //Arrange            
            var actualSelectedPort = SerialPort.GetPortNames().First();
            var expectedDisconnectedLog = $"Successfully disconnected from {actualSelectedPort}.";
            _fixture.Initialize(initOpenPort: true);

            //Act
            Thread.Sleep(500);
            _fixture.ConnectViewModel.DisconnectCommand.Execute().Subscribe();
            Thread.Sleep(500);

            //Assert
            _fixture.ConnectViewModel.Log.Logs.Should().Contain(expectedDisconnectedLog);
        }

        [Fact]
        public void Given_Connected_When_Pinged_RespondsWithSuccessLog()
        {
            //Arrange            
            var expectedPingLog = $"PingCommand Started";
            var expectedPongLog = $"PingCommand Completed (Success)";
            _fixture.Initialize(initOpenPort: true);

            //Act
            Thread.Sleep(500);
            _fixture.ConnectViewModel.PingCommand.Execute().Subscribe();
            Thread.Sleep(500);

            _fixture.ConnectViewModel.Log.Logs.Should().Contain(expectedPingLog);
            _fixture.ConnectViewModel.Log.Logs.Should().Contain(expectedPongLog);
        }


        [Fact]
        public void Given_Connected_When_Disconnecting_And_Reconnecting_Then_Ping_RespondsWithSuccessLog()
        {
            //Arrange            
            var expectedPingLog = $"PingCommand Started";
            var expectedPongLog = $"PingCommand Completed (Success)";
            _fixture.Initialize(initOpenPort: true);

            //Act
            Thread.Sleep(500);
            _fixture.ConnectViewModel.DisconnectCommand.Execute().Subscribe();
            Thread.Sleep(500);
            _fixture.ConnectViewModel.ConnectCommand.Execute().Subscribe();
            Thread.Sleep(500);
            _fixture.ConnectViewModel.PingCommand.Execute().Subscribe();
            Thread.Sleep(500);
            _fixture.ConnectViewModel.Log.Logs.Should().Contain(expectedPingLog);
            _fixture.ConnectViewModel.Log.Logs.Should().Contain(expectedPongLog);
        }

        [Fact]
        public void Given_NotConnected_Then_ConnectionStatusIsFalse()
        {
            //Arrange            
            _fixture.Initialize(initOpenPort: false);

            //Assert
            _fixture.ConnectViewModel.IsConnected.Should().BeFalse();
        }

        [Fact]
        public void Given_Connected_When_Disconnected_Then_ConnectionStatusIsFalse()
        {
            //Arrange            
            _fixture.Initialize(initOpenPort: true);

            //Act
            Thread.Sleep(500);
            _fixture.ConnectViewModel.DisconnectCommand.Execute().Subscribe();
            Thread.Sleep(500);

            //Assert
            _fixture.ConnectViewModel.IsConnected.Should().BeFalse();
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
            //Act
            _fixture.Initialize(initOpenPort: true);
            Thread.Sleep(500);

            //Assert
            _fixture.ConnectViewModel.IsConnected.Should().BeTrue();
        }


        [Fact]
        public void Given_Connected_When_ResetClicked_Then_LogsReset()
        {
            //Arrange
            var expectedLog1 = "Reset cmd received";
            var expectedLog2 = "Loading IO handler: TeensyROM";
            var expectedLog3 = "Resetting C64";
            _fixture.Initialize(initOpenPort: true);
            Thread.Sleep(1000);

            //Act            
            _fixture.ConnectViewModel.ResetCommand.Execute().Subscribe();
            Thread.Sleep(1000);


            //Assert
            _fixture.ConnectViewModel.Log.Logs.Should().Contain(expectedLog1);
            _fixture.ConnectViewModel.Log.Logs.Should().Contain(expectedLog2);
            _fixture.ConnectViewModel.Log.Logs.Should().Contain(expectedLog3);
        }

        public void Dispose()
        {
            _fixture.Dispose();
        }
    }
}