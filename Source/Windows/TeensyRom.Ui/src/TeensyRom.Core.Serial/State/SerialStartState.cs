using System.Reactive;
using TeensyRom.Core.Abstractions;

namespace TeensyRom.Core.Serial.State
{
    public class SerialStartState(IObservableSerialPort _serialPort) : SerialState(_serialPort)
    {
        public override bool CanTransitionTo(Type nextStateType)
        {
            return nextStateType == typeof(SerialConnectableState);
        }
        public override void StartPortPoll() => _serialPort.StartPortPoll();
        public override Unit SetPort(string port) => _serialPort.SetPort(port);
    }
}
