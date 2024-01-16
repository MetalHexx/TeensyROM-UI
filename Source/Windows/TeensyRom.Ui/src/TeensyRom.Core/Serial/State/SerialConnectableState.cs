namespace TeensyRom.Core.Serial.State
{
    public class SerialConnectableState : ISerialState
    {
        public bool CanTransitionTo(Type nextStateType)
        {
            return nextStateType == typeof(SerialConnectedState) 
                || nextStateType == typeof(SerialStartState);
        }

        public void Handle(SerialStateContext context)
        {
            throw new NotImplementedException();
        }
    }
}
