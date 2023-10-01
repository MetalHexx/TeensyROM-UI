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
        public readonly SerialPort _serialPort = new SerialPort { ReadTimeout = 200 };

        public void SetPort(string port)
        {
            _serialPort.PortName = port;
        }
    }
}