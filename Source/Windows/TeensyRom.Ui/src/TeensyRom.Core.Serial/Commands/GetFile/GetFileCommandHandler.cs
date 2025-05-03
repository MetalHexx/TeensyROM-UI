using MediatR;
using TeensyRom.Core.Common;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Core.Commands.GetFile
{
    public class GetFileCommandHandler(ISerialStateContext serialState) : IRequestHandler<GetFileCommand, GetFileResult>
    {
        public async Task<GetFileResult> Handle(GetFileCommand r, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            serialState.SendIntBytes(TeensyToken.GetFile, 2);
            serialState.HandleAck();
            serialState.SendIntBytes(r.StorageType.GetStorageToken(), 1);
            serialState.Write($"{r.FilePath}\0");
            
            var fileLength = serialState.ReadIntBytes(4);
            var checksum = serialState.ReadIntBytes(4);
            var buffer = GetFileBytes(fileLength);
            serialState.HandleAck();

            var receivedChecksum = CalculateChecksum(buffer);

            if (receivedChecksum != checksum)
            {
                return new GetFileResult { IsSuccess = false, Error = "Checksum Mismatch" };
            }
            return new GetFileResult { FileData = buffer };
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
                bytesRead += serialState.Read(buffer, bytesRead, fileLengthInt - bytesRead);
            }

            return buffer;
        }
        private ushort CalculateChecksum(byte[] data)
        {
            uint checksum = 0;
            foreach (var b in data)
            {
                checksum += b;
            }
            return (ushort)(checksum & 0xffff);
        }
    }
}