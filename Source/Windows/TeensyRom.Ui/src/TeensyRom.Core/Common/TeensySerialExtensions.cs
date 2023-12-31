using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Serial;

namespace TeensyRom.Core.Common
{
    public static class TeensySerialExtensions
    {
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
