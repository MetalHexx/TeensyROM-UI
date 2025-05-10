using System.Reactive;

namespace TeensyRom.Core.Abstractions
{
    public interface ISerialStateContext : IObservableSerialPort
    {
        IObservable<ISerialState> CurrentState { get; }

        void TransitionTo(Type nextStateType);
    }
}