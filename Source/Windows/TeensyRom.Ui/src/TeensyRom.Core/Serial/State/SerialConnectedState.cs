namespace TeensyRom.Core.Serial.State
{
    public class SerialConnectedState : ISerialState
    {
        public bool CanTransitionTo(Type nextStateType)
        {
            return nextStateType == typeof(SerialConnectableState) 
                || nextStateType == typeof(SerialBusyState)
                || nextStateType == typeof(SerialConnectionLostState);
        }

        public void Handle(SerialStateContext context)
        {
            return;
        }
    }
}
