namespace TeensyRom.Core.Serial
{
    public interface ISerialPortState
    {
        IObservable<bool> IsBusy { get; }
        IObservable<bool> IsConnected { get; }
        IObservable<string[]> Ports{ get; }
        IObservable<bool> IsRetryingConnection { get; }
    }

    public class SerialPortState(IObservableSerialPort _serialPort) : ISerialPortState
    {
        public IObservable<bool> IsBusy => _serialPort.IsLocked;
        public IObservable<bool> IsConnected => _serialPort.IsConnected;
        public IObservable<string[]> Ports => _serialPort.Ports;
        public IObservable<bool> IsRetryingConnection => _serialPort.IsRetryingConnection;
    }
}
