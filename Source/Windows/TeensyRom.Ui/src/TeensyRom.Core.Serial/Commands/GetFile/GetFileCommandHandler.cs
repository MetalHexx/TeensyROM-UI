using MediatR;
using TeensyRom.Core.Common;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Entities.Storage;
using System.IO.Ports;

namespace TeensyRom.Core.Commands.GetFile
{
    public class GetFileCommandHandler(ISerialStateContext serial) : IRequestHandler<GetFileCommand, GetFileResult>
    {
        public async Task<GetFileResult> Handle(GetFileCommand r, CancellationToken cancellationToken)
        {
            serial.ClearBuffers();
            serial.SendIntBytes(TeensyToken.GetFile, 2);
            serial.HandleAck();
            serial.SendIntBytes(r.StorageType.GetStorageToken(), 1);
            serial.Write($"{r.FilePath}\0");

            try
            {
                serial.HandleAck();
            }
            catch (Exception ex)
            {
                return new GetFileResult
                {
                    IsSuccess = false,
                    Error = $"Error fetching {r.FilePath}."
                };
            }

            var fileLength = serial.ReadIntBytes(4);
            var checksum = serial.ReadIntBytes(4);
            var buffer = GetFileBytes(fileLength);
            serial.HandleAck();

            var receivedChecksum = buffer.CalculateChecksum();

            if (receivedChecksum != checksum)
            {
                throw new TeensyException("Checksum Mismatch");
            }
            return new GetFileResult 
            {
                FileData = buffer
            };
        }

        private byte[] GetFileBytes(uint fileLength)
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