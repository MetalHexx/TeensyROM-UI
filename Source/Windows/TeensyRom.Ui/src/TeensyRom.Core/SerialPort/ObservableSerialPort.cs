using System.IO.Ports;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace TeensyRom.Core.Serial
{
    public class ObservableSerialPort : IObservableSerialPort
    {   
        private readonly BehaviorSubject<string[]> _ports = new(SerialPort.GetPortNames());
        public IObservable<string[]> Ports => _ports.AsObservable();

        private readonly BehaviorSubject<string> _logs = new("Not connected.");
        public IObservable<string> Logs => _logs.AsObservable();

        public readonly SerialPort _serialPort = new SerialPort { ReadTimeout = 200 };


        public void SetPort(string port)
        {
            _serialPort.PortName = port;
        }

        public Unit EnsureConnection()
        {
            if (_serialPort.IsOpen) return Unit.Default;

            _logs.OnNext($"Connecting to {_serialPort.PortName}.");
            try
            {
                _serialPort.Open();
                _logs.OnNext($"Connection to {_serialPort.PortName} successful.");
            }
            catch (Exception ex)
            {
                _logs.OnNext($"Failed to open the serial port: {ex.Message}");
            }
            return Unit.Default;
        }
    }
}