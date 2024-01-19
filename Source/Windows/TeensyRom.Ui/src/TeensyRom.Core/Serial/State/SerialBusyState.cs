using TeensyRom.Core.Logging;

namespace TeensyRom.Core.Serial.State
{
    public class SerialBusyState(IObservableSerialPort _serialPort) : SerialState(_serialPort)
    {
        public override bool CanTransitionTo(Type nextStateType)
        {
            return nextStateType == typeof(SerialConnectedState)
                || nextStateType == typeof(SerialConnectionLostState);
        }

        public override void Handle(SerialStateContext context)
        {
            throw new NotImplementedException();
        }

        public override int BytesToRead { get; }
        public override void Write(string text) => _serialPort.Write(text);
        public override void Write(byte[] buffer, int offset, int count) => _serialPort.Write(buffer, offset, count);   
        public override void Write(char[] buffer, int offset, int count) => _serialPort.Write(buffer, offset, count);
        public override void SendIntBytes(uint intToSend, short numBytes) => _serialPort.SendIntBytes(intToSend, numBytes);
        public override void Unlock() => _serialPort.Unlock();
        public override int Read(byte[] buffer, int offset, int count) => _serialPort.Read(buffer, offset, count);
        public override int ReadByte() => _serialPort.ReadByte();
        public override string ReadSerialAsString(int msToWait = 0) => _serialPort.ReadSerialAsString(msToWait);
        public override byte[] ReadSerialBytes() => _serialPort.ReadSerialBytes();
        public override void WaitForSerialData(int numBytes, int timeoutMs) => _serialPort.WaitForSerialData(numBytes, timeoutMs);
    }
}
