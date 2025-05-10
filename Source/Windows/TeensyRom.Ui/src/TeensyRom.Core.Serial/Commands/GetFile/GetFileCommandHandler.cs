using MediatR;
using TeensyRom.Core.Common;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Abstractions;

namespace TeensyRom.Core.Commands.GetFile
{
    public enum GetFileErrorCode
    {
        StorageParamError = 1,
        PathParamError = 2,
        StorageUnavailable = 3,
        FileNotFound = 4,
        FileOpenError = 5,
        UnknownError = 6
    }

    public class GetFileCommandHandler() : IRequestHandler<GetFileCommand, GetFileResult>
    {
        private ISerialStateContext _serial = null!;
        public async Task<GetFileResult> Handle(GetFileCommand r, CancellationToken cancellationToken)
        {
            _serial = r.Serial;

            _serial.ClearBuffers();
            _serial.SendIntBytes(TeensyToken.GetFile, 2);
            _serial.HandleAck();
            _serial.SendIntBytes(r.StorageType.GetStorageToken(), 1);
            _serial.Write($"{r.FilePath}\0");

            try
            {
                _serial.HandleAck();
            }
            catch (Exception ex)
            {
                GetFileErrorCode errorCode = ex.Message switch
                {
                    string msg when msg.Contains("Error 1") => GetFileErrorCode.StorageParamError,
                    string msg when msg.Contains("Error 2") => GetFileErrorCode.PathParamError,
                    string msg when msg.Contains("Error 3") => GetFileErrorCode.StorageUnavailable,
                    string msg when msg.Contains("Error 4") => GetFileErrorCode.FileNotFound,
                    string msg when msg.Contains("Error 5") => GetFileErrorCode.FileOpenError,
                    _ => GetFileErrorCode.UnknownError
                };
                return new GetFileResult
                {
                    IsSuccess = false,
                    Error = ex.Message,
                    ErrorCode = errorCode
                };
            }

            var fileLength = _serial.ReadIntBytes(4);
            var checksum = _serial.ReadIntBytes(4);
            var buffer = GetFileBytes(fileLength);
            _serial.HandleAck();

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
                bytesRead += _serial.Read(buffer, bytesRead, fileLengthInt - bytesRead);
            }

            return buffer;
        }
    }
}