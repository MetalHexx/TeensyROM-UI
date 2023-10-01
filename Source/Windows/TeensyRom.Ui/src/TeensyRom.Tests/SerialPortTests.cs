using FluentAssertions;
using System.IO.Ports;
using TeensyRom.Core.Serial;
using TeensyRom.Ui.Features.Connect;

namespace TeensyRom.Tests
{
    public class SerialPortTests
    {
        private ConnectViewModel _viewModel;
        public SerialPortTests()
        {
            _viewModel = new ConnectViewModel(new ObservableSerialPort());
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

            //Assert
            _viewModel.Logs.Should().Contain(expectedErrorLog);
        }
    }
}