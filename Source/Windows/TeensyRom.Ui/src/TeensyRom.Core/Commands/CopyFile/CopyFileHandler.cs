using MediatR;
using System.Reactive.Linq;
using TeensyRom.Core.Common;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands
{
    public class CopyFileHandler : IRequestHandler<CopyFileCommand, CopyFileResult>
    {
        private readonly IObservableSerialPort _serialPort;
        private TeensySettings _settings;

        public CopyFileHandler(IObservableSerialPort serialPort, ISettingsService settings)
        {
            settings.Settings.Take(1).Subscribe(s => _settings = s);
            _serialPort = serialPort;
        }

        public Task<CopyFileResult> Handle(CopyFileCommand request, CancellationToken cancellationToken)
        {
            _serialPort.SendIntBytes(TeensyToken.CopyFile, 2);
            _serialPort.SendIntBytes(_settings.TargetType.GetStorageToken(), 1);
            _serialPort.Write($"{request.SourcePath}\0");
            _serialPort.Write($"{request.DestPath}\0");
            _serialPort.HandleAck();
            return Task.FromResult(new CopyFileResult());
        }
    }
}