using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Serial;

namespace TeensyRom.Core.Commands.PlaySubtune
{
    public interface IPlaySubtuneSerialRoutine
    {
        void Execute(uint subtuneIndex);
    }

    public class PlaySubtuneSerialRoutine(ISerialStateContext serialState) : IPlaySubtuneSerialRoutine
    {
        public void Execute(uint subtuneIndex)
        {
            subtuneIndex = subtuneIndex > 0
                ? subtuneIndex - 1
                : 0;

            serialState.ClearBuffers();
            serialState.SendIntBytes(TeensyToken.PlaySubtune, 2);
            serialState.SendIntBytes(subtuneIndex, 1);
            serialState.HandleAck();
        }
    }
}
