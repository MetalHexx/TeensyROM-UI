using System.Reactive;
using System.Reactive.Linq;

namespace TeensyRom.Core.Serial
{
    public class TeensyObservableSerialPort : ObservableSerialPort, ITeensyObservableSerialPort
    {
        public Unit PingDevice()
        {
            if (!_serialPort.IsOpen)
            {
                _logs.OnNext("You must first connect in order to ping the device.");
                return Unit.Default;
            }
            _logs.OnNext($"Pinging device");

            _serialPort.Write(TeensySerialPortConstants.Ping_Bytes.ToArray(), 0, 2);

            return Unit.Default;
        }
    }
}