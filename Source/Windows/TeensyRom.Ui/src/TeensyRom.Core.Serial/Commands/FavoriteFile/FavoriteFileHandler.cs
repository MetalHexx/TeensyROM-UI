using MediatR;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Settings;

namespace TeensyRom.Core.Commands
{
    public class FavoriteFileHandler(ISerialStateContext serialState) : IRequestHandler<FavoriteFileCommand, FavoriteFileResult>
    {
        public Task<FavoriteFileResult> Handle(FavoriteFileCommand request, CancellationToken cancellationToken)
        {
            serialState.SendIntBytes(TeensyToken.CopyFile, 2);
            serialState.HandleAck();
            serialState.SendIntBytes(request.StorageType.GetStorageToken(), 1);
            serialState.Write($"{request.SourcePath}\0");
            serialState.Write($"{request.TargetPath}\0");
            serialState.HandleAck();
            return Task.FromResult(new FavoriteFileResult());
        }
    }
}