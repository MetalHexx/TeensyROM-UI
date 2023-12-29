using MediatR;
using System.Reactive;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Settings;

namespace TeensyRom.Core.Commands
{
    public class PingCommand : IRequest { }

    public class PingCommandHandler : TeensyCommand, IRequestHandler<PingCommand>
    {
        public PingCommandHandler(ISettingsService settingsService, IObservableSerialPort serialPort, ILoggingService logService)
            : base(settingsService, serialPort, logService) { }

        public Task Handle(PingCommand request, CancellationToken cancellationToken)
        {
            if (!_serialPort.IsOpen)
            {
                _logService.Log("You must first connect in order to ping the device.");
                return Task.CompletedTask;
            }
            _logService.Log($"Pinging device");

            _serialPort.Write(TeensyConstants.Ping_Bytes.ToArray(), 0, 2);

            return Task.CompletedTask;
        }
    }
}
