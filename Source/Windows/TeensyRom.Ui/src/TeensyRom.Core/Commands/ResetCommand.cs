using MediatR;
using System.IO;
using System.Reactive;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands
{
    public record ResetCommand : IRequest { }

    public class ResetCommandHandler : TeensyCommand, IRequestHandler<ResetCommand>
    {
        public ResetCommandHandler(ISettingsService settingsService, IObservableSerialPort serialPort, ILoggingService logService)
            : base(settingsService, serialPort, logService) { }


        public Task Handle(ResetCommand request, CancellationToken cancellationToken)
        {
            if (!_serialPort.IsOpen)
            {
                _logService.Log("You must first connect in order to reset the device.");
            }
            _logService.Log($"Resetting device");

            _serialPort.Write(TeensyConstants.Reset_Bytes.ToArray(), 0, 2);
            return Task.CompletedTask;
        }
    }
}
