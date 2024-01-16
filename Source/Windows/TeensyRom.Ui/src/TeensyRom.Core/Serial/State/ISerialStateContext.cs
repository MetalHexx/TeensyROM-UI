
using System.Reactive;

namespace TeensyRom.Core.Serial.State
{
    public interface ISerialStateContext
    {
        ISerialState CurrentState { get; }
        IObservable<string[]> Ports { get; }
        int BytesToRead { get; }

        Unit ClosePort();
        void Handle();
        void Lock();
        Unit OpenPort();
        int Read(byte[] buffer, int offset, int count);
        string ReadSerialAsString(int msToWait = 0);
        void SendIntBytes(uint intToSend, short byteLength);
        Unit SetPort(string port);
        void TransitionTo(Type nextStateType);
        void TransitionToPreviousState();
        void Unlock();
        void WaitForSerialData(int numBytes, int timeoutMs);
        void Write(string text);
        void Write(char[] buffer, int offset, int count);
        void Write(byte[] buffer, int offset, int count);
    }
}