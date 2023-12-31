using MediatR;
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
            settings.Settings.Subscribe(s => _settings = s);
            _serialPort = serialPort;
        }

        public Task<LaunchFileResult> Handle(LaunchFileCommand request, CancellationToken cancellationToken)
        {
            _serialPort.SendIntBytes(TeensyToken.LaunchFile, 2);

            if (_serialPort.GetAck() != TeensyToken.Ack)
            {
                _serialPort.ReadSerialAsString();
                throw new TeensyException("Error getting acknowledgement when Launch File Token sent");
            }
            _serialPort.SendIntBytes(_settings.TargetType.GetStorageToken(), 1);
            _serialPort.Write($"{request.Path}\0");

            if (_serialPort.GetAck() != TeensyToken.Ack)
            {
                _serialPort.ReadSerialAsString(msToWait: 100);
                throw new TeensyException("Error getting acknowledgement when launch path sent");
            }
            return Task.FromResult(new LaunchFileResult());
        }
    }
}