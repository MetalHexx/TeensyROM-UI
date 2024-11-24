namespace TeensyRom.Ui.Core.Serial.State
{
    public class SerialConnectionLostState : SerialState
    {
        public SerialConnectionLostState(IObservableSerialPort serialPort) : base(serialPort) { }        
        public override bool CanTransitionTo(Type nextStateType)
        {
            return nextStateType == typeof(SerialConnectedState);
        }
    }
}
