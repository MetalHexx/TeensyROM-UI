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
    }
}
