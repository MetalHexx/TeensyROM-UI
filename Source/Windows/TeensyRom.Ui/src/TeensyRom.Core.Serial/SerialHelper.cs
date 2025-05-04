﻿using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Common;
using TeensyRom.Core.Serial.State;

namespace TeensyRom.Core.Serial
{
    public static class SerialHelper

    {
        public static TeensyToken HandleAck(this ISerialStateContext serialState)
        {
            var response = serialState.GetAck();


            if (response == TeensyToken.Ack) return TeensyToken.Ack;
            if (response == TeensyToken.RetryLaunch) return TeensyToken.RetryLaunch;

            var rawResponse = serialState.ReadAndLogSerialAsString();

            if (rawResponse.Contains("Busy!"))
            {
                throw new TeensyBusyException($"TeensyROM is currently busy.  If you have a program runnning, stop it first. Try caching your files.");
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
            serialState.WaitForSerialData(numBytes: 2, timeoutMs: 10000);

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
        public static bool IsTeensyRom(this string response)
        {
            return response.Contains("teensyrom", StringComparison.OrdinalIgnoreCase)
                || response.Contains("busy", StringComparison.OrdinalIgnoreCase);
        }

        public static List<string> GetPorts() => SerialPort.GetPortNames().Distinct().ToList();
    }
}
