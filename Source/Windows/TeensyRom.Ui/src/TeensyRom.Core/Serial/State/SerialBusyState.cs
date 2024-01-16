namespace TeensyRom.Core.Serial.State
{
    public class SerialBusyState : ISerialState
    {
        public bool CanTransitionTo(Type nextStateType)
        {
            return nextStateType == typeof(SerialConnectedState);
        }

        public void Handle(SerialStateContext context)
        {
            throw new NotImplementedException();
        }
    }
}
