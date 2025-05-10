using System.Reactive;

namespace TeensyRom.Core.Abstractions
{
    public interface ISerialState
    {
        int BytesToRead { get; }
        IObservable<string[]> Ports { get; }

        bool CanTransitionTo(Type nextStateType);
        void ClearBuffers();
        Unit ClosePort();
        void Dispose();
        void EnsureConnection(int waitTimeMs = 200);
        void Lock();
        string? OpenPort();
        int Read(byte[] buffer, int offset, int count);
        string ReadAndLogSerialAsString(int msToWait = 0);
        int ReadByte();
        uint ReadIntBytes(short byteLength);
        string ReadSerialAsString(int msToWait = 0);
        byte[] ReadSerialBytes();
        byte[] ReadSerialBytes(int msToWait = 0);
        void SendIntBytes(uint intToSend, short numBytes);
        void SendSignedChar(sbyte charToSend);
        void SendSignedShort(short value);
        Unit SetPort(string port);
        string? StartHealthCheck();
        void StartPortPoll();
        void StopHealthCheck();
        void Unlock();
        void WaitForSerialData(int numBytes, int timeoutMs);
        void Write(byte[] buffer, int offset, int count);
        void Write(char[] buffer, int offset, int count);
        void Write(string text);
    }
}