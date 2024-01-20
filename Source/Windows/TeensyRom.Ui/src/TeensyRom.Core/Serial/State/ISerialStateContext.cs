
using System.Reactive;

namespace TeensyRom.Core.Serial.State
{
    public interface ISerialStateContext: IObservableSerialPort
    {
        IObservable<SerialState> CurrentState { get; }

        void TransitionTo(Type nextStateType);
    }
}