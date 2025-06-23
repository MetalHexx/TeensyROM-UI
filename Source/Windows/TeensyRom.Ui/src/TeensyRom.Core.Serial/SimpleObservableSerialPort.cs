using System.IO.Ports;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Common;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial.State;

namespace TeensyRom.Core.Serial
{
    public class SimpleObservableSerialPort(ILoggingService log, IAlertService alert) : IObservableSerialPort
    {
        public IObservable<string[]> Ports => _ports.AsObservable();
        public IObservable<Type> State => _state.AsObservable();

        private readonly BehaviorSubject<string[]> _ports = new(SerialPort.GetPortNames());
        private readonly BehaviorSubject<Type> _state = new(typeof(SerialStartState));

        public int BytesToRead => _serialPort.BytesToRead;

        public bool IsOpen => _serialPort.IsOpen;

        private readonly SerialPort _serialPort = new() 
        {
            Encoding = Encoding.UTF8,
            BaudRate = 115200 
        };

        public void ClearBuffers() => _serialPort.ClearBuffers();
        public int Read(byte[] buffer, int offset, int count) => _serialPort.Read(buffer, offset, count);
        public string ReadAndLogSerialAsString(int msToWait = 0) => ReadSerialAsString(msToWait);
        public int ReadByte() => _serialPort.ReadByte();
        public uint ReadIntBytes(short byteLength) => _serialPort.ReadIntBytes(byteLength);
        public string ReadSerialAsString(int msToWait = 0) => _serialPort.ReadSerialAsString(msToWait);
        public byte[] ReadSerialBytes() => _serialPort.ReadSerialBytes();
        public byte[] ReadSerialBytes(int msToWait = 0) => _serialPort.ReadSerialBytes(msToWait);
        public void SendIntBytes(uint intToSend, short numBytes) => _serialPort.SendIntBytes(intToSend, numBytes);
        public void SendSignedChar(sbyte charToSend) => _serialPort.SendSignedChar(charToSend);
        public void SendSignedShort(short value) => _serialPort.SendSignedShort(value);
        public void WaitForSerialData(int numBytes, int timeoutMs) => _serialPort.WaitForSerialData(numBytes, timeoutMs);
        public void Write(string text) => _serialPort.Write(text);
        public void Write(byte[] buffer, int offset, int count) => _serialPort.Write(buffer, offset, count);
        public void Write(char[] buffer, int offset, int count) => _serialPort.Write(buffer, offset, count);

        public Unit SetPort(string port)
        {
            if (string.IsNullOrWhiteSpace(port)) 
            {
                throw new TeensyException("Port cannot be empty.");
            }
            var ports = SerialPort.GetPortNames();

            if (!ports.Contains(port))
            {
                throw new TeensyException("The set port is currently unavailable");
            }
            _serialPort.PortName = port;

            _state.OnNext(typeof(SerialConnectableState));

            return Unit.Default;
        }

        public string? OpenPort()
        {
            StartHealthCheck();            
            return _serialPort.PortName;
        }
        public Unit ClosePort()
        {
            StopHealthCheck();
            log.Internal($"Disconnecting from {_serialPort.PortName}.");

            if (_serialPort.IsOpen) _serialPort.Close();

            _state.OnNext(typeof(SerialConnectableState));

            log.InternalSuccess($"Successfully disconnected from {_serialPort.PortName}.");

            return Unit.Default;
        }        

        public void EnsureConnection(int waitTimeMs = 200)
        {
            if (_serialPort.IsOpen) return;

            Lock();
            
            if (_serialPort.IsOpen) _serialPort.Close();

            log.Internal($"ObservableSerialPort.EnsureConnection: Attempting to open {_serialPort.PortName}");

            var failureMessage = $"ObservableSerialPort.EnsureConnection: Unable to connect to {_serialPort.PortName}";

            try
            {
                _serialPort.Open();
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception)
            {
                log.ExternalError(failureMessage);
                throw new TeensyException(failureMessage);
            }
            if (!_serialPort.IsOpen) 
            {
                log.ExternalError(failureMessage);
                throw new TeensyException(failureMessage);
            }

            log.InternalSuccess($"ObservableSerialPort.EnsureConnection: Successfully connected to {_serialPort.PortName}");

            Unlock();
            return;
        }

        private void ReadAndLogStaleBuffer()
        {
            log.Internal("ObservableSerialPort.ReadAndLogStaleBuffer: Reading and logging any remaining bytes in the buffer.");
            var bytes = ReadSerialBytes();
            var logEntry = bytes.ToLogString();

            if (string.IsNullOrWhiteSpace(logEntry)) return;

            log.External(logEntry);
        }

        public void Lock()
        {
            //log.Internal("ObservableSerialPort.Lock: Locking serial port to prevent interruptions of command processing.");

            _serialPort.ClearBuffers();

            _serialEventSubscription?.Dispose();
            _serialEventSubscription = null;
        }

        public void Unlock()
        {
            //log.Internal("ObservableSerialPort.Unlock: Unlocking serial port");

            _serialEventSubscription?.Dispose();

            _serialEventSubscription = Observable.FromEventPattern<SerialDataReceivedEventHandler, SerialDataReceivedEventArgs>
            (
                handler => _serialPort.DataReceived += handler,
                handler => _serialPort.DataReceived -= handler
            )
            .Select(serialEvent => ReadSerialBytes())
            .Where(bytes => bytes.Length > 0)
            .Select(bytes => bytes.ToLogString())
            .Where(log => !string.IsNullOrWhiteSpace(log))
            .Publish()
            .RefCount()
            .Subscribe(logs => log.External(logs));
        }

        public string? StartHealthCheck()
        {
            //_healthCheckSubscription?.Dispose();

            try
            {
                EnsureConnection();

                if (_serialPort.IsOpen)
                {
                    _state.OnNext(typeof(SerialConnectedState));
                }
                else
                {
                    _state.OnNext(typeof(SerialConnectionLostState));
                }
            }
            catch (Exception) { }

            //_healthCheckSubscription = Observable
            //    .Interval(TimeSpan.FromMilliseconds(SerialPortConstants.Health_Check_Milliseconds))
            //    .SelectMany(_ => Observable.Defer(() =>
            //    {
            //        try
            //        {
            //            if (_serialPort.IsOpen)
            //            {
            //                if (_state.Value != typeof(SerialConnectedState))
            //                {
            //                    _state.OnNext(typeof(SerialConnectedState));
            //                }
            //                return Observable.Empty<long>();
            //            }
            //            var showDisconnectedMessage = _serialPort.IsOpen == false
            //                && _state.Value != typeof(SerialConnectionLostState)
            //                && _state.Value != typeof(SerialStartState);

            //            if (showDisconnectedMessage)
            //            {
            //                alert.Publish($"Connection to {_serialPort.PortName} was lost.");
            //            }
            //            _state.OnNext(typeof(SerialConnectionLostState));

            //            EnsureConnection();

            //            _state.OnNext(typeof(SerialConnectedState));
            //        }
            //        catch (UnauthorizedAccessException)
            //        {
            //        }
            //        catch (Exception ex)
            //        {
            //            _state.OnNext(typeof(SerialConnectionLostState));
            //            log.ExternalError($"Connection to {_serialPort.PortName} was lost.");
            //            return Observable.Throw<long>(ex);
            //        }                    
            //        return Observable.Empty<long>();
            //    }))
            //    .RetryWhen(ex => ex.DelaySubscription(TimeSpan.FromMilliseconds(SerialPortConstants.Health_Check_Milliseconds)))
            //    .Subscribe();

            //if (_serialPort.IsOpen)
            //{
            //    return _serialPort.PortName;
            //}
            return null;
        }

        public void StartPortPoll()
        {
            //string[] initialPorts = SerialPort.GetPortNames();

            //if (initialPorts.Length > 0) _state.OnNext(typeof(SerialConnectableState));

            //_portRefresherSubscription = Observable
            //    .Interval(TimeSpan.FromMilliseconds(SerialPortConstants.Health_Check_Milliseconds))
            //    .Select(_ => SerialPort.GetPortNames())
            //    .Scan(initialPorts, (previousPorts, currentPorts) =>
            //    {
            //        var previousHasPorts = previousPorts.Length > 0;
            //        var currentHasPorts = currentPorts.Length > 0;

            //        if (currentPorts.Length == 0)
            //        {
            //            log.InternalError($"Failed to find connectable ports.  Check your USB connection to the TeensyROM cart and make sure your C64 is turned on.");
            //            _state.OnNext(typeof(SerialStartState));
            //        }
            //        else if (!previousHasPorts && currentPorts.Length > 0)
            //        {
            //            log.InternalSuccess("Successfully located connectable ports.");
            //            _state.OnNext(typeof(SerialConnectableState));
            //        }
            //        return currentPorts;
            //    })
            //    .Subscribe(_ports.OnNext);
        }

        public void StopHealthCheck()
        {
            _healthCheckSubscription?.Dispose();
        }

        public void Dispose()
        {
            if (_serialPort.IsOpen) _serialPort.Close();

            _serialPort?.Dispose();
            _ports?.Dispose();
            _portRefresherSubscription?.Dispose();
            _healthCheckSubscription?.Dispose();
            _serialEventSubscription?.Dispose();
        }

        private IDisposable? _serialEventSubscription;
        private IDisposable? _healthCheckSubscription;
        private IDisposable? _portRefresherSubscription;
    }
}
