using MediatR;
using System.Reactive.Linq;
using TeensyRom.Core.Common;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands.File.LaunchFile
{
    public class LaunchFileHandler: IRequestHandler<LaunchFileCommand, LaunchFileResult>
    {
        private TeensySettings _settings;
        private readonly ISerialStateContext _serialState;

        public LaunchFileHandler(ISerialStateContext serialState, ISettingsService settings)
        {
            settings.Settings.Take(1).Subscribe(s => _settings = s);
            _serialState = serialState;
        }

        public Task<LaunchFileResult> Handle(LaunchFileCommand request, CancellationToken cancellationToken)
        {
            _serialState.SendIntBytes(TeensyToken.LaunchFile, 2);

            _serialState.HandleAck();
            _serialState.SendIntBytes(_settings.TargetType.GetStorageToken(), 1);
            _serialState.Write($"{request.Path}\0");
            _serialState.HandleAck();
            return Task.FromResult(new LaunchFileResult());
        }
    }
}