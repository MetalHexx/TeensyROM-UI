using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive;
using TeensyRom.Core.Logging;

namespace TeensyRom.Core.Serial.State
{
    public class SerialStateContext : ReactiveObject, ISerialStateContext, IObservableSerialPort
    {
        [Reactive] public SerialState CurrentState { get; private set; }
        public IObservable<string[]> Ports => CurrentState.Ports;
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
            CurrentState = _states[typeof(SerialStartState)];
            _stateSubscription = _serialPort.State.Subscribe(TransitionTo);
            _serialPort.StartPortPoll();
        }

        public void TransitionTo(Type nextStateType)
        {
            if (nextStateType == CurrentState.GetType())
            {
                _log.Internal($"Already in a {CurrentState.GetType().Name}");
                return;
            }

            if (CurrentState.CanTransitionTo(nextStateType))
            {
                CurrentState = _states[nextStateType];
                _log.Internal($"Transitioned to state: {CurrentState.GetType().Name}");
                return;
            }
            _log.InternalError($"Transition to {nextStateType.Name} is not allowed from {CurrentState.GetType().Name}");
        }

        public Unit OpenPort() => CurrentState.OpenPort();
        public Unit ClosePort() => CurrentState.ClosePort();
        public Unit SetPort(string port) => CurrentState.SetPort( port);
        public void Lock() => CurrentState.Lock();
        public void Unlock() => CurrentState.Unlock();
        public void SendIntBytes(uint intToSend, short byteLength) => CurrentState.SendIntBytes(intToSend, byteLength);
        public void Write(string text) => CurrentState.Write(text);
        public void Write(char[] buffer, int offset, int count) => CurrentState.Write(buffer, offset, count);
        public void Write(byte[] buffer, int offset, int count) => CurrentState.Write(buffer, offset, count);
        public void WaitForSerialData(int numBytes, int timeoutMs) => CurrentState.WaitForSerialData(numBytes, timeoutMs);
        public string ReadSerialAsString(int msToWait = 0) => CurrentState.ReadSerialAsString(msToWait);
        public int BytesToRead => CurrentState.BytesToRead;
        public int Read(byte[] buffer, int offset, int count) => CurrentState.Read(buffer, offset, count);
        public int ReadByte() => CurrentState.ReadByte();
        public byte[] ReadSerialBytes() => CurrentState.ReadSerialBytes();
        public void StartPortPoll() => CurrentState.StartPortPoll();
        public void Dispose()
        {
            foreach (var state in _states.Values)
            {
                state.Dispose();
            }
            _serialPort.Dispose();
            _stateSubscription.Dispose();
        }
        IObservable<Type> IObservableSerialPort.State => throw new NotImplementedException();
    }
}
