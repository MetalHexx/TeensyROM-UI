using System.IO.Ports;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using TeensyRom.Core.Logging;

namespace TeensyRom.Core.Serial
{
    /// <summary>
    /// Serial port wrapper to provide access to serial port operations.
    /// Provides observables that can be used to monitor serial activity. 
    /// Resiliency routines are employed to recover from a disconnection.
    /// </summary>
    public class ObservableSerialPort : IObservableSerialPort
    {
        public IObservable<string[]> Ports => _ports.AsObservable();
        public IObservable<bool> IsConnected => _isConnected.AsObservable();
        public IObservable<bool> IsRetryingConnection => _isRetryingConnection.AsObservable();

        protected readonly BehaviorSubject<string[]> _ports = new(SerialPort.GetPortNames());
        protected readonly BehaviorSubject<bool> _isConnected = new(false);
        protected readonly BehaviorSubject<bool> _isRetryingConnection = new(false);

        protected Subject<string> _serialData = new Subject<string>();

        protected IDisposable? _dataSubscription;
        protected IDisposable? _logSubscription;
        protected IDisposable? _healthCheckSubscription;
        protected IDisposable? _portRefresherSubscription;
        protected IObservable<byte[]>? _rawSerialBytes;

        public readonly SerialPort _serialPort = new() { BaudRate = 115200 };
        protected readonly ILoggingService _logService;

        public bool IsOpen => _serialPort.IsOpen;
        public int BytesToRead => _serialPort.BytesToRead;
        public string[] GetPortNames() => SerialPort.GetPortNames();
        public void Write(string text) => _serialPort.Write(text);
        public void Write(byte[] buffer, int offset, int count) => _serialPort.Write(buffer, offset, count);
        public void Write(char[] buffer, int offset, int count) => _serialPort.Write(buffer, offset, count);
        public int Read(byte[] buffer, int offset, int count) => _serialPort.Read(buffer, offset, count);
        public int ReadByte() => _serialPort.ReadByte();

        public ObservableSerialPort(ILoggingService logService)
        {
            _logService = logService;
            StartPortPoll();
            EnableAutoReadStream();
        }

        public void SetPort(string port)
        {
            if (string.IsNullOrWhiteSpace(port))
            {
                _logService.Log("Set a port to get connected.");
                return;
            }
            _serialPort.PortName = port;
        }

        public Unit OpenPort()
        {
            EnsureConnection();

            _healthCheckSubscription = Observable
                .Interval(TimeSpan.FromMilliseconds(SerialPortConstants.Health_Check_Milliseconds))
                .SelectMany(_ => Observable.Defer(() =>
                {
                    try
                    {
                        EnsureConnection();
                        _isRetryingConnection.OnNext(false);
                    }
                    catch (Exception ex)
                    {
                        _isRetryingConnection.OnNext(true);
                        return Observable.Throw<long>(ex);
                    }
                    return Observable.Empty<long>();
                }))
                .RetryWhen(ex => ex.DelaySubscription(TimeSpan.FromMilliseconds(SerialPortConstants.Health_Check_Milliseconds)))
                .Subscribe();

            return Unit.Default;
        }

        protected void EnsureConnection()
        {
            if (_serialPort.IsOpen) return;

            try
            {
                _serialPort.Open();

                ReadAndLogStaleBuffer();

                _isConnected.OnNext(true);
                _logService.Log($"Successfully connected to {_serialPort.PortName}");
            }
            catch
            {
                _logService.Log($"Failed to ensure the connection to {_serialPort.PortName}. Retrying in {SerialPortConstants.Health_Check_Milliseconds} ms.");
                _isConnected.OnNext(false);
                throw;
            }
        }

        public Unit ClosePort()
        {
            if (_serialPort.IsOpen) _serialPort.Close();

            _logService.Log($"Successfully disconnected from {_serialPort.PortName}.");
            _isConnected.OnNext(false);

            _healthCheckSubscription?.Dispose();

            return Unit.Default;
        }

        /// <summary>
        /// Polls the serial port and provides an observable for the currently available ports.
        /// 
        /// <remarks>
        /// When ports are not found, a error log will sent on an interval to let the user know.  
        /// To avoid spamming success, we keep track of the previous state and only do so 
        /// when recovering from a connection loss.
        /// </remarks>
        /// </summary>
        private void StartPortPoll()
        {
            var initialHasPorts = _ports.Value.Length > 0;

            _portRefresherSubscription = Observable
                .Interval(TimeSpan.FromMilliseconds(SerialPortConstants.Health_Check_Milliseconds))
                .Select(_ => SerialPort.GetPortNames().Length > 0)
                .Scan(initialHasPorts, (previousHasPorts, hasPorts) =>
                {
                    var ports = SerialPort.GetPortNames();
                    if (!hasPorts)
                    {
                        _logService.Log($"Failed to find connectable ports. Retrying in {SerialPortConstants.Health_Check_Milliseconds}.");
                        return false;
                    }
                    else if (previousHasPorts == false && hasPorts)
                    {
                        _logService.Log("Successfully located connectable ports.");
                        return true;
                    }
                    return true;
                })
                .Subscribe(_ => _ports.OnNext(SerialPort.GetPortNames()));
        }

        /// <summary>
        /// Toggle the automatic serial port read poll.  Useful when performing
        /// timing sensitive operations that might get disrupted by the polling.
        /// </summary>
        public void EnableAutoReadStream()
        {
            _rawSerialBytes = Observable.FromEventPattern<SerialDataReceivedEventHandler, SerialDataReceivedEventArgs>
            (
                handler => _serialPort.DataReceived += handler,
                handler => _serialPort.DataReceived -= handler
            )
                .Select(serialEvent => ReadSerialBytes())
                .Where(bytes => bytes.Length > 0)
                .Publish()
                .RefCount();

            _logSubscription = _rawSerialBytes
                .Select(bytes => ToLogString(bytes))
                .Subscribe(_logService.Log);
        }

        /// <summary>
        /// Disable the auto read stream.  Useful when performing timing sensitive transactions.
        /// </summary>
        public void DisableAutoReadStream()
        {
            _serialPort.DiscardInBuffer();
            _serialPort.DiscardOutBuffer();
            if (_logSubscription is not null)
            {
                _dataSubscription?.Dispose();
                _logSubscription.Dispose();
                _logSubscription = null;
                return;
            }
        }

        /// <summary>
        /// Reads the available bytes in the buffer
        /// </summary>
        protected byte[] ReadSerialBytes()
        {
            if (_serialPort.BytesToRead == 0) return Array.Empty<byte>();

            var data = new byte[_serialPort.BytesToRead];
            _serialPort.Read(data, 0, data.Length);
            return data;
        }

        public void SendIntBytes(uint intToSend, short byteLength)
        {
            var bytesToSend = BitConverter.GetBytes(intToSend);

            _logService.Log($"Sent Bytes: {BitConverter.ToString(bytesToSend)}");

            for (short byteNum = (short)(byteLength - 1); byteNum >= 0; byteNum--)
            {
                _serialPort.Write(bytesToSend, byteNum, 1);
            }
        }

        /// <summary>
        /// Helper to flush the I/O buffers
        /// </summary>
        protected void Flush()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.DiscardInBuffer();
                _serialPort.DiscardOutBuffer();
            }
        }

        /// <summary>
        /// Helper to quily read stream and write log
        /// </summary>
        protected void ReadAndLogStaleBuffer()
        {
            var bytes = ReadSerialBytes();
            var log = ToLogString(bytes);
            _logService.Log(log);
        }

        /// <summary>
        /// Converts bytes to an integer
        /// </summary>
        protected static ushort ToInt16(byte[] bytes) => (ushort)(bytes[1] * 256 + bytes[0]);

        /// <summary>
        /// Outputs bytes as a string for log output
        /// </summary>
        protected static string ToLogString(byte[] bytes) => new(bytes.Select(b => (char)b).ToArray());

        public void Dispose()
        {
            if (_serialPort.IsOpen) _serialPort.Close();

            _serialPort?.Dispose();
            _ports?.Dispose();
            _portRefresherSubscription?.Dispose();
            _healthCheckSubscription?.Dispose();
            _logSubscription?.Dispose();
            _serialData?.Dispose();
            _dataSubscription?.Dispose();
        }
    }
}