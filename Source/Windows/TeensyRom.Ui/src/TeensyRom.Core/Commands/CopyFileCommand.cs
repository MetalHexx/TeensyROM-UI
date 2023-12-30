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
            _logService.Log($"Sending copy file token: {TeensyConstants.Copy_File_Token}");
            _serialPort.SendIntBytes(TeensyConstants.Copy_File_Token, 2);

            _logService.Log($"Sending SD_nUSB: {TeensyConstants.Sd_Card_Token}");
            _serialPort.SendIntBytes(GetStorageToken(_settings.TargetType), 1);

            _logService.Log($"Sending source path: {request.SourcePath}");
            _serialPort.Write($"{request.SourcePath}\0");

            _logService.Log($"Sending destination path: {request.DestPath}");
            _serialPort.Write($"{request.DestPath}\0");

            if (!GetAck())
            {
                ReadSerialAsString(msToWait: 100);
                throw new TeensyException("Error getting acknowledgement of successful file copy");
            }
            _logService.Log("File transfer complete!");

            return Task.FromResult(new CopyFileResult());
        }
    }
}