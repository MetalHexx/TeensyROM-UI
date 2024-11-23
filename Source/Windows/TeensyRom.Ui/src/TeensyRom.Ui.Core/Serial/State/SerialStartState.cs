using System.Reactive;
using TeensyRom.Ui.Core.Common;
using TeensyRom.Ui.Core.Logging;

namespace TeensyRom.Ui.Core.Serial.State
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
