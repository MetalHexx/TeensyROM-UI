using TeensyRom.Core.Abstractions;

namespace TeensyRom.Core.Serial.Commands.ToggleMusic
{
    public interface IToggleMusicSerialRoutine
    {
        void Execute(ISerialStateContext serialState);
    }

    public class ToggleMusicSerialRoutine : IToggleMusicSerialRoutine
    {
        public void Execute(ISerialStateContext serialState)
        {
            serialState.SendIntBytes(TeensyToken.PauseMusic, 2);
            serialState.HandleAck();
        }
    }
}
