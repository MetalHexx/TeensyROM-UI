using MediatR;
using TeensyRom.Core.Common;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Serial;

namespace TeensyRom.Cli.Core.Commands.GetFile
{
    public class GetFileCommandHandler : IRequestHandler<GetFileCommand, GetFileResult>
    {
        private readonly ISerialStateContext _serialState;

        public GetFileCommandHandler(ISerialStateContext serialState)
        {
            _serialState = serialState;
        }

        public async Task<GetFileResult> Handle(GetFileCommand r, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            _serialState.SendIntBytes(TeensyToken.GetFile, 2);
            _serialState.HandleAck();
            _serialState.SendIntBytes(r.StorageType.GetStorageToken(), 1);
            _serialState.Write($"{r.FilePath}\0");
            
            var fileLength = _serialState.ReadIntBytes(4);
            var checksum = _serialState.ReadIntBytes(4);
            var buffer = GetFileBytes(fileLength);
            _serialState.HandleAck();

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
                bytesRead += _serialState.Read(buffer, bytesRead, fileLengthInt - bytesRead);
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