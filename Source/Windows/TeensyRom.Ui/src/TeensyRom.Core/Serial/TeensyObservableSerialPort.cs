using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using TeensyRom.Core.Files;
using TeensyRom.Core.Serial.Abstractions;

namespace TeensyRom.Core.Serial
{
    public class TeensyObservableSerialPort : ObservableSerialPort, ITeensyObservableSerialPort
    {
        public Unit PingDevice()
        {
            if (!_serialPort.IsOpen)
            {
                _logs.OnNext("You must first connect in order to ping the device.");
                return Unit.Default;
            }
            _logs.OnNext($"Pinging device");

            _serialPort.Write(TeensyConstants.Ping_Bytes.ToArray(), 0, 2);

            return Unit.Default;
        }

        public Unit ResetDevice()
        {
            if (!_serialPort.IsOpen)
            {
                _logs.OnNext("You must first connect in order to reset the device.");
                return Unit.Default;
            }
            _logs.OnNext($"Resetting device");

            _serialPort.Write(TeensyConstants.Reset_Bytes.ToArray(), 0, 2);

            return Unit.Default;
        }

        public bool SendFile(TeensyFileInfo fileInfo)
        {
            DisableAutoReadStream();

            _logs.OnNext($"Sending file transfer token: {TeensyConstants.Send_File_Token}");            
            SendIntBytes(TeensyConstants.Send_File_Token, 2);
            
            WaitForSerialData(numBytes: 2, timeoutMs: 500);

            if (!GetAck())
            {
                ReadSerialAsString();
                return false;
            }

            var destPath = @"/test-auto-download/";

            _logs.OnNext($"Sending Stream Length: {fileInfo.StreamLength}");
            SendIntBytes(fileInfo.StreamLength, 4);

            _logs.OnNext($"Sending Checksum: {fileInfo.Checksum}");
            SendIntBytes(fileInfo.Checksum, 2);

            _logs.OnNext($"Sending SD_nUSB: {TeensyConstants.Sd_Card_Token}");
            SendIntBytes(TeensyConstants.Sd_Card_Token, 1);

            _logs.OnNext($"Sending file path: {fileInfo.FullPath}\0");
            _serialPort.Write($"{destPath}{fileInfo.Name}\0");

            if (!GetAck())
            {
                ReadSerialAsString(msToWait: 100);
                return false;
            }
            _logs.OnNext("File ready for transfer!");

            _logs.OnNext("Sending file");
            var bytesSent = 0;

            while (fileInfo.StreamLength > bytesSent)
            {
                var bytesToSend = 16 * 1024;
                if (fileInfo.StreamLength - bytesSent < bytesToSend) bytesToSend = (int)fileInfo.StreamLength - bytesSent;
                _serialPort.Write(fileInfo.Buffer, bytesSent, bytesToSend);

                _logs.OnNext("*");
                bytesSent += bytesToSend;
            }

            if (!GetAck())
            {
                ReadSerialAsString(msToWait: 500);
                _logs.OnNext("File transfer failed.");
                return false;
            }
            _logs.OnNext("File transfer complete!");

            EnableAutoReadStream();

            return true;
        }

        public void ReadSerial()
        {
            if (_serialPort.BytesToRead == 0) return;

            byte[] receivedData = new byte[_serialPort.BytesToRead];
            _serialPort.Read(receivedData, 0, receivedData.Length);

            _logs.OnNext("Received Bytes: " + BitConverter.ToString(receivedData));
        }

        public void ReadSerialAsString(int msToWait = 0)
        {
            Thread.Sleep(msToWait);
            if (_serialPort.BytesToRead == 0) return;

            byte[] receivedData = new byte[_serialPort.BytesToRead];
            _serialPort.Read(receivedData, 0, receivedData.Length);

            _logs.OnNext("Received String: " + Encoding.ASCII.GetString(receivedData));
        }

        public void SendIntBytes(uint intToSend, short byteLength)
        {
            var bytesToSend = BitConverter.GetBytes(intToSend);

            _logs.OnNext($"Sent Bytes: {BitConverter.ToString(bytesToSend)}");

            for (short byteNum = (short)(byteLength - 1); byteNum >= 0; byteNum--)
            {
                _serialPort.Write(bytesToSend, byteNum, 1);
            }
        }

        public bool GetAck()
        {
            WaitForSerialData(numBytes: 2, timeoutMs: 500);

            byte[] recBuf = new byte[2];
            _serialPort.Read(recBuf, 0, 2);
            ushort recU16 = ToInt16(recBuf);

            switch (recU16)
            {
                case TeensyConstants.Ack_Token:
                    _logs.OnNext("Response: Acknowledgement Token Received");
                    return true;

                case TeensyConstants.Fail_Token:
                    _logs.OnNext("Response: Acknowledgement Failure Received");
                    return false;

                default:
                    _logs.OnNext("Response: Unexpected Response that was not an Ack token - " + recBuf[0].ToString("X2") + ":" + recBuf[1].ToString("X2"));
                    return false;
            }
        }

        private void WaitForSerialData(int numBytes, int timeoutMs)
        {
            var sw = new Stopwatch();
            sw.Start();

            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                if (_serialPort.BytesToRead >= numBytes) return;
                Thread.Sleep(10);
            }
            throw new TimeoutException("Timed out waiting for data to be received");
        }
    }
}