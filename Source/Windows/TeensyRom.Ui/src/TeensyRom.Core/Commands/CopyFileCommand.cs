using MediatR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Common;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands
{
    public class CopyFileCommand : IRequest<CopyFileResult> 
    {
        public string SourcePath { get; init; } = string.Empty;
        public string DestPath { get; init; } = string.Empty;
    }

    public class CopyFileResult : CommandResult { }

    public class CopyFileHandler : TeensyCommand, IRequestHandler<CopyFileCommand, CopyFileResult>
    {

        public CopyFileHandler(
            ISettingsService settingsService, 
            IObservableSerialPort serialPort, 
            ILoggingService logService) 
            : base(settingsService, serialPort, logService) { }

        public Task<CopyFileResult> Handle(CopyFileCommand request, CancellationToken cancellationToken)
        {
            _serialPort.SendIntBytes(TeensyToken.CopyFile.Value, 2);
            _serialPort.SendIntBytes(_settings.TargetType.GetStorageToken(), 1);
            _serialPort.Write($"{request.SourcePath}\0");
            _serialPort.Write($"{request.DestPath}\0");

            if (_serialPort.GetAck() != TeensyToken.Ack)
            {
                _serialPort.ReadSerialAsString(msToWait: 100);
                throw new TeensyException("Error getting acknowledgement of successful file copy");
            }
            _logService.Log("File transfer complete!");

            return Task.FromResult(new CopyFileResult());
        }
    }
}