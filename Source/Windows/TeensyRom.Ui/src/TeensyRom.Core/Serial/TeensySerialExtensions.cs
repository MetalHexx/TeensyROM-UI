using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Common;
using TeensyRom.Core.Serial.State;

namespace TeensyRom.Core.Serial
{
    public static class TeensySerialExtensions
    {
        public static void HandleAck(this ISerialStateContext serialState)
        {
            var response = serialState.GetAck();

            if (response != TeensyToken.Ack)
            {
                var dataString = serialState.ReadSerialAsString();
                
                var responseString = response switch
                {
                    var _ when response == TeensyToken.Fail => "Fail Token",
                    _ => "Unknown",
                };
                if(dataString.Length == 0) dataString = "No Data";
                throw new TeensyException($"Received unexpected response from TR ({responseString}) with data: {dataString}");
            }
        }
        public static TeensyToken GetAck(this ISerialStateContext serialState)
        {
            serialState.WaitForSerialData(numBytes: 2, timeoutMs: 500);

            byte[] recBuf = new byte[2];
            serialState.Read(recBuf, 0, 2);
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
