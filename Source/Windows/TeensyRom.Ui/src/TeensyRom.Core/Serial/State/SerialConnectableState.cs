namespace TeensyRom.Core.Serial.State
{
    public class SerialConnectableState : SerialState
    {
        public override bool CanTransitionTo(Type nextStateType)
        {
            return nextStateType == typeof(SerialConnectedState) 
                || nextStateType == typeof(SerialStartState);
        }

        public override void Handle(SerialStateContext context)
        {
            throw new NotImplementedException();
        }
    }
}
