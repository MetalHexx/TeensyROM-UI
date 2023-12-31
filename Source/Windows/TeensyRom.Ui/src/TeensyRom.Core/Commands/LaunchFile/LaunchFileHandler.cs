using MediatR;
using System.Reactive.Linq;
using TeensyRom.Core.Common;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands.File.LaunchFile
{
    public class LaunchFileHandler: IRequestHandler<LaunchFileCommand, LaunchFileResult>
    {
        private TeensySettings _settings;
        private readonly IObservableSerialPort _serialPort;

        public LaunchFileHandler(IObservableSerialPort serialPort, ISettingsService settings)
        {
            settings.Settings.Take(1).Subscribe(s => _settings = s);
            _serialPort = serialPort;
        }

        public Task<LaunchFileResult> Handle(LaunchFileCommand request, CancellationToken cancellationToken)
        {
            _serialPort.SendIntBytes(TeensyToken.LaunchFile, 2);

            _serialPort.HandleAck();
            _serialPort.SendIntBytes(_settings.TargetType.GetStorageToken(), 1);
            _serialPort.Write($"{request.Path}\0");
            _serialPort.HandleAck();
            return Task.FromResult(new LaunchFileResult());
        }
    }
}