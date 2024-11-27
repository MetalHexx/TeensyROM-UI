using MediatR;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands.FavoriteFile
{
    public class FavoriteFileHandler : IRequestHandler<FavoriteFileCommand, FavoriteFileResult>
    {
        private readonly ISerialStateContext _serialState;

        public FavoriteFileHandler(ISerialStateContext serialState)
        {
            _serialState = serialState;
        }

        public Task<FavoriteFileResult> Handle(FavoriteFileCommand request, CancellationToken cancellationToken)
        {
            _serialState.SendIntBytes(TeensyToken.CopyFile, 2);
            _serialState.SendIntBytes(request.StorageType.GetStorageToken(), 1);
            _serialState.Write($"{request.SourcePath}\0");
            _serialState.Write($"{request.TargetPath}\0");
            _serialState.HandleAck();
            return Task.FromResult(new FavoriteFileResult());
        }
    }
}