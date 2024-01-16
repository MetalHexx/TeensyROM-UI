namespace TeensyRom.Core.Serial.State
{
    public class SerialStartState : SerialState
    {
        public override bool CanTransitionTo(Type nextStateType)
        {
            return nextStateType == typeof(SerialConnectableState);
        }

        public override void Handle(SerialStateContext context)
        {
            throw new NotImplementedException();
        }
    }
}
