namespace TeensyRom.Core.Serial.State
{
    public class SerialConnectionLostState : SerialState
    {
        public override bool CanTransitionTo(Type nextStateType)
        {
            return nextStateType == typeof(SerialConnectedState);
        }

        public override void Handle(SerialStateContext context)
        {
            context.TransitionTo(typeof(SerialConnectedState));
        }
    }
}
