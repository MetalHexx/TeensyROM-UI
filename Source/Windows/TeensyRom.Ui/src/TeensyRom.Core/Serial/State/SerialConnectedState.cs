namespace TeensyRom.Core.Serial.State
{
    public class SerialConnectedState : SerialState
    {
        public override bool CanTransitionTo(Type nextStateType)
        {
            return nextStateType == typeof(SerialConnectableState) 
                || nextStateType == typeof(SerialBusyState)
                || nextStateType == typeof(SerialConnectionLostState);
        }

        public override void Handle(SerialStateContext context)
        {
            throw new NotImplementedException();
        }
    }
}
