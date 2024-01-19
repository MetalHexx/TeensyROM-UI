using System.Reactive;
using TeensyRom.Core.Logging;

namespace TeensyRom.Core.Serial.State
{
    public class SerialConnectableState : SerialState
    {
        public SerialConnectableState(IObservableSerialPort _serialPort) : base(_serialPort) { }
        public override bool CanTransitionTo(Type nextStateType)
        {
            return nextStateType == typeof(SerialConnectedState) 
                || nextStateType == typeof(SerialStartState);
        }

        public override void Handle(SerialStateContext context)
        {
            throw new NotImplementedException();
        }
        public override Unit SetPort(string port) => _serialPort.SetPort(port);
        public override Unit OpenPort() => _serialPort.OpenPort();
    }
}
