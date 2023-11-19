namespace TeensyRom.Core.Serial
{
    public interface ISerialPortState
    {
        IObservable<bool> IsConnected { get; }
        IObservable<string[]> Ports{ get; }
        IObservable<bool> IsRetryingConnection { get; }
    }

    public class SerialPortState : ISerialPortState
    {
        public IObservable<bool> IsConnected => _serialPort.IsConnected;
        public IObservable<string[]> Ports => _serialPort.Ports;
        public IObservable<bool> IsRetryingConnection => _serialPort.IsRetryingConnection;
        private readonly IObservableSerialPort _serialPort;

        public SerialPortState(IObservableSerialPort serialPort)
        {
            _serialPort = serialPort;
        }
    }
}
