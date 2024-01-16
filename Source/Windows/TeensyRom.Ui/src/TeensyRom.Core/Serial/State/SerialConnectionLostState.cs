namespace TeensyRom.Core.Serial.State
{
    public class SerialConnectionLostState : ISerialState
    {
        public bool CanTransitionTo(Type nextStateType)
        {
            return nextStateType == typeof(SerialConnectedState);
        }

        public void Handle(SerialStateContext context)
        {
            context.TransitionTo(typeof(SerialConnectedState));
        }
    }
}
