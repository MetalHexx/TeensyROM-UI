using MediatR;
using System.ComponentModel;
using System.Reactive;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Settings;

namespace TeensyRom.Core.Commands
{
    public class PingCommand : IRequest<PingResult> { }
    public class PingResult : CommandResult { }

    public class PingCommandHandler : TeensyCommand, IRequestHandler<PingCommand, PingResult>
    {
        public PingCommandHandler(ISettingsService settingsService, IObservableSerialPort serialPort, ILoggingService logService)
            : base(settingsService, serialPort, logService) { }

        public Task<PingResult> Handle(PingCommand request, CancellationToken cancellationToken)
        {
            if (!_serialPort.IsOpen)
            {
                return Task.FromResult(new PingResult 
                {
                    Error = "You must first connect in order to ping the device."
                });
            }
            _serialPort.Write(TeensyConstants.Ping_Bytes.ToArray(), 0, 2);

            return Task.FromResult(new PingResult());
        }
    }
}
