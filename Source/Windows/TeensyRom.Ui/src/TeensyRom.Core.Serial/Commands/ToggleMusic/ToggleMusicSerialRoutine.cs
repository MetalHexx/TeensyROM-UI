using TeensyRom.Core.Serial.State;

namespace TeensyRom.Core.Serial.Commands.ToggleMusic
{
    public interface IToggleMusicSerialRoutine
    {
        void Execute();
    }

    public class ToggleMusicSerialRoutine(ISerialStateContext serialState) : IToggleMusicSerialRoutine
    {
        public void Execute()
        {
            serialState.SendIntBytes(TeensyToken.PauseMusic, 2);
            serialState.HandleAck();
        }
    }
}
