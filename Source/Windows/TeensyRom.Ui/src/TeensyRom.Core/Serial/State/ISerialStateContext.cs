
using System.Reactive;

namespace TeensyRom.Core.Serial.State
{
    public interface ISerialStateContext: IObservableSerialPort
    {
        SerialState CurrentState { get; }
        void TransitionTo(Type nextStateType);
    }
}