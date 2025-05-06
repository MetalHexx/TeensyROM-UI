using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Commands.GetFile;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Serial.State;

namespace TeensyRom.Core.Serial.Commands.GetFile
{
    public static class GetFileSerialRoutine
    {
        public static byte[] GetFile(this SerialPort serial, string filePath, TeensyStorageType storageType)
        {
            serial.ClearBuffers();
            serial.SendIntBytes(TeensyToken.GetFile, 2);
            serial.HandleAck();
            serial.SendIntBytes(storageType.GetStorageToken(), 1);
            serial.Write($"{filePath}\0");

            try
            {
                serial.HandleAck();
            }
            catch (Exception ex)
            {
                return Array.Empty<byte>();
            }

            var fileLength = serial.ReadIntBytes(4);
            var checksum = serial.ReadIntBytes(4);
            var buffer = serial.GetFileBytes(fileLength);
            serial.HandleAck();

            var receivedChecksum = buffer.CalculateChecksum();

            if (receivedChecksum != checksum)
            {
                throw new TeensyException("Checksum Mismatch");
            }
            return buffer;
        }

        private static byte[] GetFileBytes(this SerialPort serial, uint fileLength)
        {
            if (!fileLength.TryParseInt(out int fileLengthInt))
            {
                throw new TeensyException("The file size attempting to be fetched is too large.");
            }

            var buffer = new byte[fileLength];
            int bytesRead = 0;

            while (bytesRead < fileLength)
            {
                bytesRead += serial.Read(buffer, bytesRead, fileLengthInt - bytesRead);
            }

            return buffer;
        }
        
    }
}
