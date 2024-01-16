using MediatR;
using System.Reactive.Linq;
using TeensyRom.Core.Common;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands
{
    public class CopyFileHandler : IRequestHandler<CopyFileCommand, CopyFileResult>
    {
        private readonly ISerialStateContext _serialState;
        private TeensySettings _settings;

        public CopyFileHandler(ISerialStateContext serialState, ISettingsService settings)
        {
            settings.Settings.Take(1).Subscribe(s => _settings = s);
            _serialState = serialState;
        }

        public Task<CopyFileResult> Handle(CopyFileCommand request, CancellationToken cancellationToken)
        {
            _serialState.SendIntBytes(TeensyToken.CopyFile, 2);
            _serialState.SendIntBytes(_settings.TargetType.GetStorageToken(), 1);
            _serialState.Write($"{request.SourcePath}\0");
            _serialState.Write($"{request.DestPath}\0");
            _serialState.HandleAck();
            return Task.FromResult(new CopyFileResult());
        }
    }
}