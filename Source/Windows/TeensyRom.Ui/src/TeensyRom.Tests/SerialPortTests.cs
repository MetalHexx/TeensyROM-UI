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
    }
}