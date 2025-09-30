using MediatR;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Serial;

namespace TeensyRom.Core.Commands
{
    public class CopyFileHandler : IRequestHandler<CopyFileCommand, CopyFileResult>
    {
        public Task<CopyFileResult> Handle(CopyFileCommand request, CancellationToken cancellationToken)
        {
            request.Serial.SendIntBytes(TeensyToken.CopyFile, 2);
            request.Serial.HandleAck();
            request.Serial.SendIntBytes(request.StorageType.GetStorageToken(), 1);
            request.Serial.Write($"{request.SourcePath}\0");
            request.Serial.Write($"{request.DestPath}\0");
            request.Serial.HandleAck();
            return Task.FromResult(new CopyFileResult());
        }
    }
}