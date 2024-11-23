namespace TeensyRom.Cli.Core.Serial.State
{
    public class SerialBusyState(IObservableSerialPort _serialPort) : SerialState(_serialPort)
    {
        public override bool CanTransitionTo(Type nextStateType)
        {
            return nextStateType == typeof(SerialConnectedState)
                || nextStateType == typeof(SerialConnectionLostState)
                || nextStateType == typeof(SerialConnectableState);
        }
        public override int BytesToRead => _serialPort.BytesToRead;
        public override void Write(string text) => _serialPort.Write(text);
        public override void Write(byte[] buffer, int offset, int count) => _serialPort.Write(buffer, offset, count);   
        public override void Write(char[] buffer, int offset, int count) => _serialPort.Write(buffer, offset, count);
        public override void SendIntBytes(uint intToSend, short numBytes) => _serialPort.SendIntBytes(intToSend, numBytes);
        public override uint ReadIntBytes(short byteLength) => _serialPort.ReadIntBytes(byteLength);
        public override void Lock() => _serialPort.Lock();
        public override void Unlock() => _serialPort.Unlock();
        public override int Read(byte[] buffer, int offset, int count) => _serialPort.Read(buffer, offset, count);
        public override int ReadByte() => _serialPort.ReadByte();
        public override string ReadAndLogSerialAsString(int msToWait = 0) => _serialPort.ReadAndLogSerialAsString(msToWait);
        public override byte[] ReadSerialBytes(int msToWait = 0) => _serialPort.ReadSerialBytes(msToWait);
        public override string ReadSerialAsString(int msToWait = 0) => _serialPort.ReadSerialAsString(msToWait);
        public override byte[] ReadSerialBytes() => _serialPort.ReadSerialBytes();
        public override void WaitForSerialData(int numBytes, int timeoutMs) => _serialPort.WaitForSerialData(numBytes, timeoutMs);
        public override void ClearBuffers() => _serialPort.ClearBuffers();
        public override void StartHealthCheck() => _serialPort.StartHealthCheck();
        public override void StopHealthCheck() => _serialPort.StopHealthCheck();
        public override void EnsureConnection() => _serialPort.EnsureConnection();
    }
}
