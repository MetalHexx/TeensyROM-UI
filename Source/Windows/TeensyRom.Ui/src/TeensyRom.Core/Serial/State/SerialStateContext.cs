using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive;
using TeensyRom.Core.Logging;

namespace TeensyRom.Core.Serial.State
{
    public class SerialStateContext : ReactiveObject, ISerialStateContext
    {
        [Reactive]
        public SerialState CurrentState { get; private set; }
        public SerialState? PreviousState { get; private set; }
        public IObservable<string[]> Ports => _serialPort.Ports;

        private readonly Dictionary<Type, SerialState> _states = new()
        {
                { typeof(SerialStartState), new SerialStartState() },
                { typeof(SerialConnectableState), new SerialConnectableState() },
                { typeof(SerialConnectedState), new SerialConnectedState() },
                { typeof(SerialBusyState), new SerialBusyState() },
                { typeof(SerialConnectionLostState), new SerialConnectionLostState() }
        };
        private readonly ILoggingService _log;
        private IObservableSerialPort _serialPort;

        public SerialStateContext(ILoggingService log)
        {
            CurrentState = _states[typeof(SerialStartState)];
            _log = log;
            _serialPort = new ObservableSerialPort(_log, this);
        }

        public void TransitionToPreviousState() 
        {
            if (PreviousState != null)
            {
                TransitionTo(PreviousState.GetType());
                return;
            }
            _log.InternalError($"Unable to transition from {CurrentState.GetType().Name} because the previous state is unknown");
        }

        public void Handle() => CurrentState.Handle(this);

        public void TransitionTo(Type nextStateType)
        {
            if (nextStateType == CurrentState.GetType())
            {
                _log.Internal($"Already in a {CurrentState.GetType().Name}");
                return;
            }

            if (CurrentState.CanTransitionTo(nextStateType))
            {
                PreviousState = CurrentState;
                CurrentState = _states[nextStateType];
                _log.Internal($"Transitioned to state: {CurrentState.GetType().Name}");
                return;
            }
            _log.InternalError($"Transition to {nextStateType.Name} is not allowed from {CurrentState.GetType().Name}");
        }

        public Unit OpenPort() => _serialPort.OpenPort();
        public Unit ClosePort() => _serialPort.ClosePort();
        public Unit SetPort(string port) => _serialPort.SetPort( port);
        public void Lock() => _serialPort.Lock();
        public void Unlock() => _serialPort.Unlock();

        public void SendIntBytes(uint intToSend, short byteLength) => _serialPort.SendIntBytes(intToSend, byteLength);
        public void Write(string text) => _serialPort.Write(text);

        public void Write(char[] buffer, int offset, int count) => _serialPort.Write(buffer, offset, count);
        public void Write(byte[] buffer, int offset, int count) => _serialPort.Write(buffer, offset, count);

        public void WaitForSerialData(int numBytes, int timeoutMs) => _serialPort.WaitForSerialData(numBytes, timeoutMs);
        public string ReadSerialAsString(int msToWait = 0) => _serialPort.ReadSerialAsString(msToWait);
        public int BytesToRead => _serialPort.BytesToRead;
        public int Read(byte[] buffer, int offset, int count) => _serialPort.Read(buffer, offset, count);
    }
}
