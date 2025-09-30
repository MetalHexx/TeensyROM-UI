using MediatR;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Serial;

namespace TeensyRom.Core.Commands
{
    public class FavoriteFileHandler : IRequestHandler<FavoriteFileCommand, FavoriteFileResult>
    {
        public Task<FavoriteFileResult> Handle(FavoriteFileCommand request, CancellationToken cancellationToken)
        {
            request.Serial.SendIntBytes(TeensyToken.CopyFile, 2);
            request.Serial.HandleAck();
            request.Serial.SendIntBytes(request.StorageType.GetStorageToken(), 1);
            request.Serial.Write($"{request.SourcePath}\0");
            request.Serial.Write($"{request.TargetPath}\0");
            request.Serial.HandleAck();
            return Task.FromResult(new FavoriteFileResult());
        }
    }
}