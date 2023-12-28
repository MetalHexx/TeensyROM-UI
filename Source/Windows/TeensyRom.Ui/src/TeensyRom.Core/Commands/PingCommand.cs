using MediatR;
using System.Reactive;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Settings;

namespace TeensyRom.Core.Commands
{
    public class PingRequest : IRequest { }

    public class PingCommand : TeensyCommand, IRequestHandler<PingRequest>
    {
        public PingCommand(ISettingsService settingsService, IObservableSerialPort serialPort, ILoggingService logService)
            : base(settingsService, serialPort, logService) { }

        public Task Handle(PingRequest request, CancellationToken cancellationToken)
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
