namespace TeensyRom.Core.Serial.State
{
    public class SerialBusyState : SerialState
    {
        public override bool CanTransitionTo(Type nextStateType)
        {
            return nextStateType == typeof(SerialConnectedState);
        }

        public override void Handle(SerialStateContext context)
        {
            throw new NotImplementedException();
        }
    }
}
