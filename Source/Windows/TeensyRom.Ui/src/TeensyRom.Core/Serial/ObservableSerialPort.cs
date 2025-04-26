﻿using System.Diagnostics;
using System.IO.Ports;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using TeensyRom.Core.Common;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial.State;

namespace TeensyRom.Core.Serial
{
    /// <summary>
    /// Serial port wrapper to provide access to serial port operations.
    /// Provides observables that can be used to monitor serial activity. 
    /// Resiliency routines are employed to recover from a disconnection.
    /// </summary>
    public class ObservableSerialPort(ILoggingService _log, IAlertService _alert) : IObservableSerialPort
    {
        public IObservable<Type> State => _state.AsObservable();
        public IObservable<string[]> Ports => _ports.AsObservable();
        
        private readonly BehaviorSubject<string[]> _ports = new(SerialPort.GetPortNames());
        private readonly BehaviorSubject<Type> _state = new(typeof(SerialStartState));
        private readonly SerialPort _serialPort = new() { BaudRate = 115200 };
        private bool _healthCheckEnabled = true;

        public int BytesToRead => _serialPort.BytesToRead;
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

        public void EnsureConnection()
        {            
            if (_serialPort.IsOpen) return;

            Lock();

            var ports = SerialPort.GetPortNames().Distinct();

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
                    Write(TeensyByteToken.Ping_Bytes.ToArray(), 0, 2);                    
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

                var ms = 4000;

                _log.Internal($"ObservableSerialPort.EnsureConnection: Waiting {ms}ms for PING response");

                var response = ReadAndLogSerialAsString(ms);

                var isTeensyResponse = response.Contains("teensyrom", StringComparison.OrdinalIgnoreCase)
                    || response.Contains("busy", StringComparison.OrdinalIgnoreCase);

                if (!isTeensyResponse)
                {
                    _log.ExternalError($"ObservableSerialPort.EnsureConnection: PING failed -- TeensyROM was not detected on {_serialPort.PortName}");
                    continue;
                }
                if(response.Contains("minimal", StringComparison.OrdinalIgnoreCase))
                {
                    _alert.Publish($"Detected TeensyROM minimal mode. You've been reconnected to {_serialPort.PortName}");
                }
                else
                {
                    _alert.Publish($"Connected to TeensyROM on {_serialPort.PortName}");
                }
                _log.Internal($"ObservableSerialPort.EnsureConnection: PING succeeded");
                
                ReadAndLogStaleBuffer();
                Unlock();
                return;
            }
            if (_serialPort.IsOpen) _serialPort.Close();

            Unlock();

            throw new TeensyException($"ObservableSerialPort.EnsureConnection: Failed to ensure the connection to {_serialPort.PortName}. Retrying in {SerialPortConstants.Health_Check_Milliseconds} ms.");
        }

        public Unit OpenPort()
        {
            StartHealthCheck();
            return Unit.Default;
        }

        public void StartHealthCheck() 
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

            if (_serialEventSubscription is not null) return;

            _serialEventSubscription = Observable.FromEventPattern<SerialDataReceivedEventHandler, SerialDataReceivedEventArgs>
            (
                handler => _serialPort.DataReceived += handler,
                handler => _serialPort.DataReceived -= handler
            )
            .Select(serialEvent => ReadSerialBytes())
            .Where(bytes => bytes.Length > 0)
            .Select(ToLogString)
            .Where(log => !string.IsNullOrWhiteSpace(log))
            .Publish()
            .RefCount()
            .Subscribe(_log.External);
        }

        public void Lock()
        {
            _log.Internal("ObservableSerialPort.Lock: Locking serial port to prevent interruptions of command processing.");

            ClearBuffers();

            _serialEventSubscription?.Dispose();
        }

        /// <summary>
        /// Reads the available bytes in the buffer
        /// </summary>
        public byte[] ReadSerialBytes()
        {
            if (_serialPort.BytesToRead == 0) return [];

            var data = new byte[_serialPort.BytesToRead];
            _serialPort.Read(data, 0, data.Length);
            return data;
        }

        public byte[] ReadSerialBytes(int msToWait = 0)
        {
            Thread.Sleep(msToWait);
            if (_serialPort.BytesToRead == 0) return [];

            byte[] receivedData = new byte[_serialPort.BytesToRead];
            _serialPort.Read(receivedData, 0, receivedData.Length);

            return receivedData;
        }

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
        public string ReadSerialAsString(int msToWait = 0)
        {
            Thread.Sleep(msToWait);
            if (_serialPort.BytesToRead == 0) return string.Empty;

            byte[] receivedData = new byte[_serialPort.BytesToRead];
            _serialPort.Read(receivedData, 0, receivedData.Length);

            var dataString = Encoding.ASCII.GetString(receivedData);

            if (string.IsNullOrWhiteSpace(dataString)) return string.Empty;

            return dataString;
        }

        public void SendIntBytes(uint intToSend, short byteLength)
        {
            var bytesToSend = BitConverter.GetBytes(intToSend);

            for (short byteNum = (short)(byteLength - 1); byteNum >= 0; byteNum--)
            {
                _serialPort.Write(bytesToSend, byteNum, 1);
            }
        }

        public void SendSignedChar(sbyte charToSend)
        {
            byte[] byteToSend = { (byte)charToSend };
            _log.Internal($"Sending byte: {byteToSend[0]} (0x{byteToSend[0]:X2})");
            _serialPort.Write(byteToSend, 0, 1);
        }

        public uint ReadIntBytes(short byteLength)
        {
            byte[] receivedBytes = new byte[byteLength];
            int bytesReadTotal = 0;

            while (bytesReadTotal < byteLength)
            {
                int bytesRead = _serialPort.Read(receivedBytes, bytesReadTotal, byteLength - bytesReadTotal);
                if (bytesRead == 0)
                {
                    throw new TimeoutException("Timeout while reading bytes from serial port.");
                }
                bytesReadTotal += bytesRead;
            }

            uint result = 0;

            // Reconstruct the uint value in little-endian order
            for (short byteNum = 0; byteNum < byteLength; byteNum++)
            {
                result |= (uint)(receivedBytes[byteNum] << (8 * byteNum));
            }

            return result;
        }

        public void WaitForSerialData(int numBytes, int timeoutMs)
        {
            var sw = new Stopwatch();
            sw.Start();

            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                if (_serialPort.BytesToRead >= numBytes) return;
                Thread.Sleep(10);
            }
            throw new TimeoutException("Timed out waiting for data to be received");
        }

        /// <summary>
        /// Once operations are completed, this method will read and log any remaining bytes in the buffer.
        /// </summary>
        private void ReadAndLogStaleBuffer()
        {
            _log.Internal("ObservableSerialPort.ReadAndLogStaleBuffer: Reading and logging any remaining bytes in the buffer.");
            var bytes = ReadSerialBytes();
            var log = ToLogString(bytes);

            if(string.IsNullOrWhiteSpace(log)) return;  

            _log.External(log);
        }

        /// <summary>
        /// Outputs bytes as a string for log output
        /// </summary>
        private static string ToLogString(byte[] bytes) => new(bytes.Select(b => (char)b).ToArray());

        public void Dispose()
        {
            if (_serialPort.IsOpen) _serialPort.Close();

            _serialPort?.Dispose();
            _ports?.Dispose();
            _portRefresherSubscription?.Dispose();
            _healthCheckSubscription?.Dispose();
            _serialEventSubscription?.Dispose();
        }

        public void ClearBuffers()
        {
            if(!_serialPort.IsOpen) return;

            //_log.Internal("ObservableSerialPort.ClearBuffers: Clearing serial I/O buffers");
            _serialPort.DiscardInBuffer();
            _serialPort.DiscardOutBuffer();
        }

        private IDisposable? _serialEventSubscription;
        private IDisposable? _healthCheckSubscription;
        private IDisposable? _portRefresherSubscription;
    }
}