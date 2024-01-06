using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Common;

namespace TeensyRom.Core.Serial
{
    public static class TeensySerialExtensions
    {
        public static void HandleAck(this IObservableSerialPort _serialPort)
        {
            var response = _serialPort.GetAck();

            if (response != TeensyToken.Ack)
            {
                var dataString = _serialPort.ReadSerialAsString();
                
                var responseString = response switch
                {
                    var _ when response == TeensyToken.Fail => "Fail",
                    _ => "Unknown",
                };
                throw new TeensyException($"Received {responseString} Ack Token with data: {dataString}");
            }
        }
        public static TeensyToken GetAck(this IObservableSerialPort _serialPort)
        {
            _serialPort.WaitForSerialData(numBytes: 2, timeoutMs: 500);

            byte[] recBuf = new byte[2];
            _serialPort.Read(recBuf, 0, 2);
            ushort recU16 = BitConverter.ToUInt16(recBuf, 0);

            return recU16 switch
            {
                var _ when recU16 == TeensyToken.Ack.Value => TeensyToken.Ack,
                var _ when recU16 == TeensyToken.Fail.Value => TeensyToken.Fail,
                _ => TeensyToken.Unnknown,
            };
        }
    }
}
