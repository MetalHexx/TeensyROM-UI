namespace TeensyRom.Core.Serial.State
{
    public class SerialStartState : ISerialState
    {
        public bool CanTransitionTo(Type nextStateType)
        {
            return nextStateType == typeof(SerialConnectableState);
        }

        public void Handle(SerialStateContext context)
        {
            throw new NotImplementedException();
        }
    }
}
