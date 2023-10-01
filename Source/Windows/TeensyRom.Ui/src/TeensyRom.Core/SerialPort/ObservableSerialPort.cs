using System.IO.Ports;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;

namespace TeensyRom.Core.Serial
{
    public abstract class ObservableSerialPort : IObservableSerialPort, IDisposable
    {   
        protected readonly BehaviorSubject<string[]> _ports = new(SerialPort.GetPortNames());
        public IObservable<string[]> Ports => _ports.AsObservable();

        protected readonly BehaviorSubject<bool> _isConnected = new(false);
        public IObservable<bool> IsConnected => _isConnected.AsObservable();

        protected readonly BehaviorSubject<string> _logs = new("Not connected.");
        public IObservable<string> Logs => _logs.AsObservable();

        protected readonly SerialPort _serialPort = new SerialPort { ReadTimeout = 200 };
        protected IDisposable _openPortSubscription;


        public void SetPort(string port)
        {
            if (string.IsNullOrWhiteSpace(port)) return;
            _serialPort.PortName = port;
        }

        public Unit OpenPort()
        {
            _openPortSubscription?.Dispose();

            _openPortSubscription = Observable.Create<string>(observer =>
            {
                return Observable
                    .Interval(TimeSpan.FromMilliseconds(SerialPortConstants.Read_Polling_Milliseconds))
                    .SelectMany(_ => Observable.Defer(() =>
                    {

                        try
                        {
                            EnsureConnection();
                        }
                        catch (Exception ex)
                        {
                            observer.OnError(ex);
                            return Observable.Empty<string>();
                        }

                        var data = ReadBytes();

                        if (data.Length > 0)
                        {
                            observer.OnNext($"> {data}");
                        }
                        return Observable.Empty<string>();
                    }))
                    .RetryWhen(errors => errors.DelaySubscription(TimeSpan.FromSeconds(SerialPortConstants.Read_Retry_Seconds)))
                    .Subscribe();
            })
            .Subscribe(_logs.OnNext, _logs.OnError);

            return Unit.Default;
        }

        public Unit ClosePort()
        {
            _logs.OnNext($"Disconnecting from {_serialPort.PortName}.");

            if (_serialPort.IsOpen) _serialPort.Close();
            
            _logs.OnNext($"Disconnected from {_serialPort.PortName} successfully.");

            _openPortSubscription.Dispose();
            _isConnected.OnNext(false);
            return Unit.Default;
        }

        private void EnsureConnection()
        {
            if (_serialPort.IsOpen) return;

            _logs.OnNext($"Connecting to {_serialPort.PortName}.");
            try
            {
                _serialPort.Open();
                _isConnected.OnNext(true);
                _logs.OnNext($"Connection to {_serialPort.PortName} successful.");
            }
            catch (Exception ex)
            {
                _isConnected.OnNext(false);
                _logs.OnNext($"Failed to open the serial port: {ex.Message}");
                throw;
            }
        }

        private StringBuilder ReadBytes()
        {
            var data = new StringBuilder();
            while (_serialPort.BytesToRead > 0)
            {
                try
                {
                    data.AppendLine(_serialPort.ReadLine());
                }
                catch (TimeoutException)
                {
                    data.Append(".");

                    while (_serialPort.BytesToRead > 0)
                    {
                        data.Append((char)_serialPort.ReadChar());
                    }   
                    data.AppendLine();
                }
            }
            return data;
        }
        public void Dispose()
        {
            if (_serialPort.IsOpen) _serialPort.Close();

            _serialPort?.Dispose();
            _openPortSubscription?.Dispose();
            _logs?.Dispose();
            _ports?.Dispose();
        }
    }
}