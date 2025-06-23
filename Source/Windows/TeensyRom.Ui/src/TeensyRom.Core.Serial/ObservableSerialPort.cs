using System.IO.Ports;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Common;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Settings;

namespace TeensyRom.Core.Serial
{
    /// <summary>
    /// Serial port wrapper to provide access to serial port operations.
    /// Provides observables that can be used to monitor serial activity. 
    /// Resiliency routines are employed to recover from a disconnection.
    /// </summary>
    public class ObservableSerialPort(ILoggingService _log, IAlertService _alert, IFwVersionChecker versionChecker, ISettingsService settingsService) : IObservableSerialPort
    {
        public IObservable<Type> State => _state.AsObservable();
        public IObservable<string[]> Ports => _ports.AsObservable();
        
        private readonly BehaviorSubject<string[]> _ports = new(SerialPort.GetPortNames());
        private readonly BehaviorSubject<Type> _state = new(typeof(SerialStartState));
        private readonly SerialPort _serialPort = new() 
        {
            Encoding = Encoding.UTF8,
            BaudRate = 115200 
        };
        private bool _healthCheckEnabled = true;

        public int BytesToRead => _serialPort.BytesToRead;

        public bool IsOpen => _serialPort.IsOpen;

        public void Write(string text) => _serialPort.Write(text);
        public void Write(byte[] buffer, int offset, int count) => _serialPort.Write(buffer, offset, count);
        public void Write(char[] buffer, int offset, int count) => _serialPort.Write(buffer, offset, count);
        public int Read(byte[] buffer, int offset, int count) => _serialPort.Read(buffer, offset, count);
        public int ReadByte() => _serialPort.ReadByte();

        public Unit SetPort(string port)
        {
            if (string.IsNullOrWhiteSpace(port))
            {
                _log.InternalError("Set a port to get connected.");
                return Unit.Default;
            }
            _serialPort.PortName = port;
            return Unit.Default;
        }

        /// <summary>
        /// Sorts the available ports by the last known used port to increase the chance of connecting to the correct port on the first try.
        /// </summary>
        private IEnumerable<string> GetSortedPorts()
        {
            var settings = settingsService.GetSettings();
            var ports = SerialPort.GetPortNames().Distinct();

            var knownPorts = settings.KnownCarts
                .Select(cart => cart.ComPort)
                .Distinct()
                .ToList();

            return ports
                .OrderByDescending(port => knownPorts.Contains(port))
                .ThenBy(port => port);
        }

        public void EnsureConnection(int waitTimeMs = 200)
        {
            if (_serialPort.IsOpen) return;

            Lock();

            var ports = GetSortedPorts();

            foreach (var port in ports)
            {
                if (_serialPort.IsOpen) _serialPort.Close();

                _serialPort.PortName = port;
                _log.Internal($"ObservableSerialPort.EnsureConnection: Attempting to open {_serialPort.PortName}");

                try
                {                     
                    _serialPort.Open();
                }
                catch(Exception)
                {
                    _log.ExternalError($"ObservableSerialPort.EnsureConnection: Unable to connect to {_serialPort.PortName}");
                    continue;
                }
                if (!_serialPort.IsOpen) continue;
                                
                _log.InternalSuccess($"ObservableSerialPort.EnsureConnection: Successfully connected to {_serialPort.PortName}");

                _log.Internal($"ObservableSerialPort.EnsureConnection: Pinging {_serialPort.PortName} to verify connection to TeensyROM");

                try
                {
                    _serialPort.WriteTimeout = 2000;
                    //_serialPort.Write([(byte)TeensyToken.VersionCheck.Value], 0, 1);
                    SendIntBytes(TeensyToken.Ping, 2);
                }
                catch (Exception)
                {
                    _log.InternalError($"ObservableSerialPort.EnsureConnection: Timed out connecting to {_serialPort.PortName}");
                    continue;
                }
                finally 
                {
                    _serialPort.WriteTimeout = SerialPort.InfiniteTimeout;
                }

                _log.Internal($"ObservableSerialPort.EnsureConnection: Waiting {waitTimeMs}ms for VERSION CHECK response");

                var response = ReadAndLogSerialAsString(waitTimeMs);

                var isTeensyResponse = response.Contains("teensyrom", StringComparison.OrdinalIgnoreCase)
                    || response.Contains("busy", StringComparison.OrdinalIgnoreCase);

                if (!isTeensyResponse)
                {
                    _log.ExternalError($"ObservableSerialPort.EnsureConnection: VERSION CHECK failed -- TeensyROM was not detected on {_serialPort.PortName}");
                    continue;
                }
                if(response.Contains("minimal", StringComparison.OrdinalIgnoreCase))
                {
                    _alert.Publish($"Detected TeensyROM minimal mode. You've been reconnected to {_serialPort.PortName}");
                }
                else
                {
                    settingsService.SetCart(_serialPort.PortName);  
                    
                    _alert.Publish($"Connected to TeensyROM on {_serialPort.PortName}");

                    if (!response.Contains("busy", StringComparison.OrdinalIgnoreCase)) 
                    {
                        versionChecker.VersionCheck(response);
                    }                    
                }
                _log.Internal($"ObservableSerialPort.EnsureConnection: VERSION CHECK succeeded");
                
                ReadAndLogStaleBuffer();
                Unlock();
                return;
            }
            if (_serialPort.IsOpen) _serialPort.Close();

            Unlock();

            throw new TeensyException($"ObservableSerialPort.EnsureConnection: Failed to ensure the connection to {_serialPort.PortName}. Retrying in {SerialPortConstants.Health_Check_Milliseconds} ms.");
        }

        public string? OpenPort()
        {
            return StartHealthCheck();
        }

        public string? StartHealthCheck() 
        {
            _healthCheckSubscription?.Dispose();

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

            _healthCheckSubscription = Observable
                .Interval(TimeSpan.FromMilliseconds(SerialPortConstants.Health_Check_Milliseconds))
                .SelectMany(_ => Observable.Defer(() =>
                {
                    try
                    {
                        if (_serialPort.IsOpen)
                        {
                            if (_state.Value != typeof(SerialConnectedState)) 
                            {
                                _state.OnNext(typeof(SerialConnectedState));
                            }
                            return Observable.Empty<long>();
                        }
                        var showDisconnectedMessage = _serialPort.IsOpen == false
                            && _state.Value != typeof(SerialConnectionLostState)
                            && _state.Value != typeof(SerialStartState);

                        if (showDisconnectedMessage) 
                        {
                            _alert.Publish($"Connection to {_serialPort.PortName} was lost.");
                        }
                        _state.OnNext(typeof(SerialConnectionLostState));

                        EnsureConnection();

                        _state.OnNext(typeof(SerialConnectedState));
                    }
                    catch (Exception ex)
                    {
                        _state.OnNext(typeof(SerialConnectionLostState));
                        _log.ExternalError($"Connection to {_serialPort.PortName} was lost.");
                        return Observable.Throw<long>(ex);
                    }
                    return Observable.Empty<long>();
                }))
                .RetryWhen(ex => ex.DelaySubscription(TimeSpan.FromMilliseconds(SerialPortConstants.Health_Check_Milliseconds)))
                .Subscribe();

            if (_serialPort.IsOpen) 
            {
                return _serialPort.PortName;
            }
            return null;
        }

        public void StopHealthCheck()
        {
            _healthCheckSubscription?.Dispose();
        }

        public Unit ClosePort()
        {
            _log.Internal($"Disconnecting from {_serialPort.PortName}.");

            if (_serialPort.IsOpen) _serialPort.Close();

            _state.OnNext(typeof(SerialConnectableState));

            _log.InternalSuccess($"Successfully disconnected from {_serialPort.PortName}.");

            _healthCheckSubscription?.Dispose();

            return Unit.Default;
        }

        public void StartPortPoll()
        {
            string[] initialPorts = SerialPort.GetPortNames();

            if (initialPorts.Length > 0) _state.OnNext(typeof(SerialConnectableState));

            _portRefresherSubscription = Observable
                .Interval(TimeSpan.FromMilliseconds(SerialPortConstants.Health_Check_Milliseconds))
                .Select(_ => SerialPort.GetPortNames())
                .Scan(initialPorts, (previousPorts, currentPorts) =>
                {
                    var previousHasPorts = previousPorts.Length > 0;
                    var currentHasPorts = currentPorts.Length > 0;

                    if (currentPorts.Length == 0)
                    {                        
                        _log.InternalError($"Failed to find connectable ports.  Check your USB connection to the TeensyROM cart and make sure your C64 is turned on.");
                        _state.OnNext(typeof(SerialStartState));
                    }
                    else if (!previousHasPorts && currentPorts.Length > 0)
                    {
                        _log.InternalSuccess("Successfully located connectable ports.");
                        _state.OnNext(typeof(SerialConnectableState));
                    }
                    return currentPorts;
                })                
                .Subscribe(_ports.OnNext);
        }

        public void Unlock()
        {
            _log.Internal("ObservableSerialPort.Unlock: Unlocking serial port");

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
            .Subscribe(logs => _log.External(logs));
        }

        public void Lock()
        {
            _log.Internal("ObservableSerialPort.Lock: Locking serial port to prevent interruptions of command processing.");

            _serialPort.ClearBuffers();

            _serialEventSubscription?.Dispose();
            _serialEventSubscription = null;
        }

        /// <summary>
        /// Reads the available bytes in the buffer
        /// </summary>
        public byte[] ReadSerialBytes() => _serialPort.ReadSerialBytes();

        public byte[] ReadSerialBytes(int msToWait = 0) => _serialPort.ReadSerialBytes(msToWait);

        public string ReadAndLogSerialAsString(int msToWait = 0)
        {
            var dataString = ReadSerialAsString(msToWait);

            if (!string.IsNullOrWhiteSpace(dataString)) 
            {
                _log.External($"{dataString}");
            }
            return dataString;
        }

        //TODO: Make it return Task<string>
        public string ReadSerialAsString(int msToWait = 0) => _serialPort.ReadSerialAsString(msToWait);

        public void SendIntBytes(uint intToSend, short byteLength) => _serialPort.SendIntBytes(intToSend, byteLength);

        public void SendSignedChar(sbyte charToSend) => _serialPort.SendSignedChar(charToSend);

        public void SendSignedShort(short value) => _serialPort.SendSignedShort(value);

        public uint ReadIntBytes(short byteLength) => _serialPort.ReadIntBytes(byteLength);

        public void WaitForSerialData(int numBytes, int timeoutMs) => _serialPort.WaitForSerialData(numBytes, timeoutMs);

        /// <summary>
        /// Once operations are completed, this method will read and log any remaining bytes in the buffer.
        /// </summary>
        private void ReadAndLogStaleBuffer()
        {
            _log.Internal("ObservableSerialPort.ReadAndLogStaleBuffer: Reading and logging any remaining bytes in the buffer.");
            var bytes = ReadSerialBytes();
            var log = bytes.ToLogString();

            if(string.IsNullOrWhiteSpace(log)) return;  

            _log.External(log);
        }

        public void ClearBuffers() => _serialPort.ClearBuffers();

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