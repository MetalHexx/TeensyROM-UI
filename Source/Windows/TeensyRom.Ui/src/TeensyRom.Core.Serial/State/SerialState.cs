using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Common;
using TeensyRom.Core.Logging;

namespace TeensyRom.Core.Serial.State
{
    public abstract class SerialState : IObservableSerialPort
    {
        public abstract bool CanTransitionTo(Type nextStateType);
        protected readonly IObservableSerialPort _serialPort;
        public SerialState(IObservableSerialPort serialPort) => _serialPort = serialPort;                
        public IObservable<string[]> Ports => _serialPort.Ports;
        public virtual int BytesToRead => throw new TeensyStateException(ExceptionMessage);
        public virtual void Write(string text) => throw new TeensyStateException(ExceptionMessage);
        public virtual void Write(byte[] buffer, int offset, int count) => throw new TeensyStateException(ExceptionMessage);
        public virtual void Write(char[] buffer, int offset, int count) => throw new TeensyStateException(ExceptionMessage);
        public virtual Unit SetPort(string port) => throw new TeensyStateException(ExceptionMessage);
        public virtual Unit OpenPort() => throw new TeensyStateException(ExceptionMessage);
        public virtual Unit ClosePort() => throw new TeensyStateException(ExceptionMessage);
        public virtual void EnsureConnection(int waitTimeMs = 200) => throw new TeensyStateException(ExceptionMessage);
        public virtual void StartHealthCheck() => throw new TeensyStateException(ExceptionMessage);
        public virtual void StopHealthCheck() => throw new TeensyStateException(ExceptionMessage);
        public virtual void SendIntBytes(uint intToSend, short numBytes) => throw new TeensyStateException(ExceptionMessage);
        public virtual void SendSignedChar(sbyte charToSend) => throw new TeensyStateException(ExceptionMessage);
        public virtual void SendSignedShort(short value) => throw new TeensyStateException(ExceptionMessage);
        public virtual uint ReadIntBytes(short byteLength) => throw new TeensyStateException(ExceptionMessage);
        public virtual void Lock() => throw new TeensyStateException(ExceptionMessage);
        public virtual void Unlock() => throw new TeensyStateException(ExceptionMessage);
        public virtual int Read(byte[] buffer, int offset, int count) => throw new TeensyStateException(ExceptionMessage);
        public virtual int ReadByte() => throw new TeensyStateException(ExceptionMessage);
        public virtual string ReadAndLogSerialAsString(int msToWait = 0) => throw new TeensyStateException(ExceptionMessage);
        public virtual string ReadSerialAsString(int msToWait = 0) => throw new TeensyStateException(ExceptionMessage);
        public virtual byte[] ReadSerialBytes() => throw new TeensyStateException(ExceptionMessage);
        public virtual void WaitForSerialData(int numBytes, int timeoutMs) => throw new TeensyStateException(ExceptionMessage);
        public virtual void StartPortPoll() => throw new TeensyStateException(ExceptionMessage);
        public virtual void Dispose() => _serialPort.Dispose();
        public virtual void ClearBuffers() => throw new NotImplementedException();

        public virtual byte[] ReadSerialBytes(int msToWait = 0) => throw new TeensyStateException(ExceptionMessage);

        protected string ExceptionMessage => $"Cannot perform serial operations in: {this.GetType().Name}";
        IObservable<Type> IObservableSerialPort.State => throw new NotImplementedException();
    }
}