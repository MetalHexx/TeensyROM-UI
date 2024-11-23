using System.Reactive;

namespace TeensyRom.Cli.Core.Serial.State
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
        public override void StopHealthCheck() => _serialPort.StopHealthCheck();
        public override void ClearBuffers() => _serialPort.ClearBuffers();
    }
}
