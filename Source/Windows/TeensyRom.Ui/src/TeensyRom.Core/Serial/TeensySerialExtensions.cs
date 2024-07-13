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
        public static TeensyToken HandleAck(this ISerialStateContext serialState)
        {
            var response = serialState.GetAck();


            if (response == TeensyToken.Ack) return TeensyToken.Ack;
            if (response == TeensyToken.RetryLaunch) return TeensyToken.RetryLaunch;

            var rawResponse = serialState.ReadAndLogSerialAsString();

            if (rawResponse.Contains("Busy!"))
            {
                throw new TeensyBusyException($"TeensyROM is currently busy and could not perform the requested operation.");
            }
            var responseString = response switch
            {
                var token when token == TeensyToken.Fail => "Fail Token",
                _ => "Unknown",
            };
            if (rawResponse.Length == 0) rawResponse = "No Data";

            throw new TeensyException($"Received unexpected response from TR ({responseString}) with data: {rawResponse}");
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
                var _ when recU16 == TeensyToken.RetryLaunch.Value => TeensyToken.RetryLaunch,
                _ => TeensyToken.Unnknown,
            };
        }
    }
}
