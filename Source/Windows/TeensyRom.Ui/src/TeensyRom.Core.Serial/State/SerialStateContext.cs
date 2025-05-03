using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using TeensyRom.Core.Logging;

namespace TeensyRom.Core.Serial.State
{
    public class SerialStateContext : ISerialStateContext, IObservableSerialPort
    {
        public IObservable<SerialState> CurrentState => _currentState.AsObservable();
        private readonly BehaviorSubject<SerialState> _currentState;
        public IObservable<string[]> Ports => _serialPort.Ports;
        private readonly Dictionary<Type, SerialState> _states;
        private readonly IObservableSerialPort _serialPort;
        private readonly ILoggingService _log;
        private readonly IDisposable _stateSubscription;

        public SerialStateContext(IObservableSerialPort serialPort, ILoggingService log)
        {
            _log = log;
            _serialPort = serialPort;            
            _states = new()
            {
                { typeof(SerialStartState), new SerialStartState(serialPort) },
                { typeof(SerialConnectableState), new SerialConnectableState(serialPort) },
                { typeof(SerialConnectedState), new SerialConnectedState(serialPort) },
                { typeof(SerialBusyState), new SerialBusyState(serialPort) },
                { typeof(SerialConnectionLostState), new SerialConnectionLostState(serialPort) }
            };
            _currentState = new(_states[typeof(SerialStartState)]);
            _stateSubscription = _serialPort.State.Subscribe(TransitionTo);
            _serialPort.StartPortPoll();
        }

        public void TransitionTo(Type nextStateType)
        {
            if (nextStateType == _currentState.Value.GetType())
            {
                //_log.Internal($"Already in a {_currentState.Value.GetType().Name}");
                return;
            }

            if (_currentState.Value.CanTransitionTo(nextStateType))
            {
                _currentState.OnNext(_states[nextStateType]);
                _log.Internal($"Transitioned to state: {_currentState.Value.GetType().Name}");
                return;
            }
            _log.InternalError($"Transition to {nextStateType.Name} is not allowed from {_currentState.Value.GetType().Name}");
        }

        public Unit OpenPort() => _currentState.Value.OpenPort();
        public Unit ClosePort() => _currentState.Value.ClosePort();
        public void StartHealthCheck() => _currentState.Value.StartHealthCheck();
        public void StopHealthCheck() => _currentState.Value.StopHealthCheck();
        public void EnsureConnection(int waitTimeMs = 200) => _currentState.Value.EnsureConnection(waitTimeMs);
        public Unit SetPort(string port) => _currentState.Value.SetPort( port);
        public void Lock() => _currentState.Value.Lock();
        public void Unlock() => _currentState.Value.Unlock();
        public void SendIntBytes(uint intToSend, short byteLength) => _currentState.Value.SendIntBytes(intToSend, byteLength);
        public void SendSignedChar(sbyte charToSend) => _currentState.Value.SendSignedChar(charToSend);
        public void SendSignedShort(short value) => _currentState.Value.SendSignedShort(value);
        public uint ReadIntBytes(short byteLength) => _currentState.Value.ReadIntBytes(byteLength);
        public void Write(string text) => _currentState.Value.Write(text);
        public void Write(char[] buffer, int offset, int count) => _currentState.Value.Write(buffer, offset, count);
        public void Write(byte[] buffer, int offset, int count) => _currentState.Value.Write(buffer, offset, count);
        public void WaitForSerialData(int numBytes, int timeoutMs) => _currentState.Value.WaitForSerialData(numBytes, timeoutMs);
        public string ReadAndLogSerialAsString(int msToWait = 0) => _currentState.Value.ReadAndLogSerialAsString(msToWait);
        public string ReadSerialAsString(int msToWait = 0) => _currentState.Value.ReadSerialAsString(msToWait);
        public int BytesToRead => _currentState.Value.BytesToRead;
        public int Read(byte[] buffer, int offset, int count) => _currentState.Value.Read(buffer, offset, count);
        public int ReadByte() => _currentState.Value.ReadByte();
        public byte[] ReadSerialBytes() => _currentState.Value.ReadSerialBytes();
        public void StartPortPoll() => _currentState.Value.StartPortPoll();
        public void Dispose()
        {
            foreach (var state in _states.Values)
            {
                state.Dispose();
            }
            _serialPort.Dispose();
            _stateSubscription.Dispose();
        }

        public void ClearBuffers()
        {
            _currentState.Value.ClearBuffers();
        }

        public byte[] ReadSerialBytes(int msToWait = 0) => _currentState.Value.ReadSerialBytes(msToWait);

        IObservable<Type> IObservableSerialPort.State => throw new NotImplementedException();
    }
}
