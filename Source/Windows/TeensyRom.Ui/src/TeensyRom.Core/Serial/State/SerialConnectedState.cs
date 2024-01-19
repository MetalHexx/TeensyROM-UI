using System.Reactive;
using TeensyRom.Core.Logging;

namespace TeensyRom.Core.Serial.State
{
    public class SerialConnectedState : SerialState
    {
        public SerialConnectedState(IObservableSerialPort serialPort) : base(serialPort) { }
        public override bool CanTransitionTo(Type nextStateType)
        {
            return nextStateType == typeof(SerialConnectableState) 
                || nextStateType == typeof(SerialBusyState)
                || nextStateType == typeof(SerialConnectionLostState);
        }
        public override Unit ClosePort() => _serialPort.ClosePort();
        public override void Lock() => _serialPort.Lock();
    }
}
