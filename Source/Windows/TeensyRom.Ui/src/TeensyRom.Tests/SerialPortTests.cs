using FluentAssertions;
using System.IO.Ports;
using TeensyRom.Core.Serial;
using TeensyRom.Ui.Features.Connect;

namespace TeensyRom.Tests
{
    public class SerialPortTests
    {
        [Fact]
        public void Should_Return_Ports()
        {
            //Arrange
            var actualSerialPorts = SerialPort.GetPortNames();
            var serialService = new ObservableSerialPort();
            var viewModel = new ConnectViewModel(serialService);

            //Assert
            viewModel.Ports.Should().BeEquivalentTo(actualSerialPorts);
        }
    }
}