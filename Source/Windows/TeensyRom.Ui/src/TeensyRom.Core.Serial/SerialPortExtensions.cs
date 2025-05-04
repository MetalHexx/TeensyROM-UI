using System.Diagnostics;
using System.IO.Ports;
using System.Text;

namespace TeensyRom.Core.Serial
{
    public static class SerialPortExtensions
    {
        public static string ReadAndLogSerialAsString(this SerialPort serial, int msToWait = 0)
        {
            var dataString = serial.ReadSerialAsString(msToWait);
            return dataString;
        }

        public static string ReadSerialAsString(this SerialPort serial, int msToWait = 0)
        {
            Thread.Sleep(msToWait);
            if (serial.BytesToRead == 0) return string.Empty;

            byte[] receivedData = new byte[serial.BytesToRead];
            serial.Read(receivedData, 0, receivedData.Length);

            var dataString = Encoding.ASCII.GetString(receivedData);

            if (string.IsNullOrWhiteSpace(dataString)) return string.Empty;

            return dataString;
        }

        public static byte[] ReadSerialBytes(this SerialPort serial)
        {
            if (serial.BytesToRead == 0) return [];

            var data = new byte[serial.BytesToRead];
            serial.Read(data, 0, data.Length);
            return data;
        }

        public static byte[] ReadSerialBytes(this SerialPort serial, int msToWait = 0)
        {
            Thread.Sleep(msToWait);
            if (serial.BytesToRead == 0) return [];

            byte[] receivedData = new byte[serial.BytesToRead];
            serial.Read(receivedData, 0, receivedData.Length);

            return receivedData;
        }

        public static void SendIntBytes(this SerialPort serial, uint intToSend, short byteLength)
        {
            var bytesToSend = BitConverter.GetBytes(intToSend);

            for (short byteNum = (short)(byteLength - 1); byteNum >= 0; byteNum--)
            {
                serial.Write(bytesToSend, byteNum, 1);
            }
        }

        public static void SendSignedChar(this SerialPort serial, sbyte charToSend)
        {
            byte[] byteToSend = { (byte)charToSend };
            serial.Write(byteToSend, 0, 1);
        }

        public static void SendSignedShort(this SerialPort serial, short value)
        {
            byte highByte = (byte)((value >> 8) & 0xFF);
            byte lowByte = (byte)(value & 0xFF);

            serial.Write([highByte], 0, 1);
            serial.Write([lowByte], 0, 1);
        }

        public static uint ReadIntBytes(this SerialPort serial, short byteLength)
        {
            byte[] receivedBytes = new byte[byteLength];
            int bytesReadTotal = 0;

            while (bytesReadTotal < byteLength)
            {
                int bytesRead = serial.Read(receivedBytes, bytesReadTotal, byteLength - bytesReadTotal);
                if (bytesRead == 0)
                {
                    throw new TimeoutException("Timeout while reading bytes from serial port.");
                }
                bytesReadTotal += bytesRead;
            }

            uint result = 0;

            // Reconstruct the uint value in little-endian order
            for (short byteNum = 0; byteNum < byteLength; byteNum++)
            {
                result |= (uint)(receivedBytes[byteNum] << (8 * byteNum));
            }

            return result;
        }

        public static void WaitForSerialData(this SerialPort serial, int numBytes, int timeoutMs)
        {
            var sw = new Stopwatch();
            sw.Start();

            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                if (serial.BytesToRead >= numBytes)
                {
                    sw.Stop();
                    //Debug.WriteLine($"WaitForSerialData - {sw.ElapsedMilliseconds}ms");
                    return;
                }
                Thread.Sleep(10);
            }
            throw new TimeoutException("Timed out waiting for data to be received");
        }

        public static void ClearBuffers(this SerialPort serial)
        {
            if (!serial.IsOpen) return;

            serial.DiscardInBuffer();
            serial.DiscardOutBuffer();
        }
    }
}
