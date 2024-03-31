using MediatR;
using System.Reactive.Linq;
using TeensyRom.Core.Commands.File.LaunchFile;
using TeensyRom.Core.Common;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands
{
    public class FavoriteFileHandler : IRequestHandler<FavoriteFileCommand, FavoriteFileResult>
    {
        private readonly ISerialStateContext _serialState;

        public FavoriteFileHandler(ISerialStateContext serialState, ISettingsService settings)
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