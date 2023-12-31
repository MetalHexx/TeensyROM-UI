using MediatR;
using System.IO;
using System.Reactive;
using TeensyRom.Core.Common;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands
{
    public class SaveFileCommand : IRequest<SaveFileResult>
    {
        public TeensyFileInfo File { get; init; } = default!;
    }

    public class SaveFileResult : CommandResult { }

    public class SaveFileCommandHandler : TeensyCommand, IRequestHandler<SaveFileCommand, SaveFileResult>
    {
        public SaveFileCommandHandler(ISettingsService settingsService, IObservableSerialPort serialPort, ILoggingService logService)
            : base(settingsService, serialPort, logService) { }

        public Task<SaveFileResult> Handle(SaveFileCommand r, CancellationToken cancellationToken)
        {
            TransformDestination(r.File);

            _serialPort.SendIntBytes(TeensyConstants.Send_File_Token, 2);

            WaitForSerialData(numBytes: 2, timeoutMs: 500);

            if (!GetAck())
            {
                _serialPort.ReadSerialAsString();
                throw new TeensyException("Error getting acknowledgement when Send File Token sent");
            }
            _serialPort.SendIntBytes(r.File.StreamLength, 4);
            _serialPort.SendIntBytes(r.File.Checksum, 2);
            _serialPort.SendIntBytes(GetStorageToken(r.File.StorageType), 1);
            _serialPort.Write($"{r.File.TargetPath.UnixPathCombine(r.File.Name)}\0");

            if (!GetAck())
            {
                _serialPort.ReadSerialAsString(msToWait: 100);
                throw new TeensyException("Error getting acknowledgement when file metadata sent");
            }
            var bytesSent = 0;

            while (r.File.StreamLength > bytesSent)
            {
                var bytesToSend = 16 * 1024;
                if (r.File.StreamLength - bytesSent < bytesToSend) bytesToSend = (int)r.File.StreamLength - bytesSent;
                _serialPort.Write(r.File.Buffer, bytesSent, bytesToSend);

                _logService.Log("*");
                bytesSent += bytesToSend;
            }

            if (!GetAck())
            {
                _serialPort.ReadSerialAsString(msToWait: 500);
                _logService.Log("File transfer failed.");
                throw new TeensyException("Error getting acknowledgement when sending file");
            }
            return Task.FromResult(new SaveFileResult());
        }

        private void TransformDestination(TeensyFileInfo fileInfo)
        {
            fileInfo.StorageType = _settings.TargetType;

            var target = _settings.FileTargets
                .FirstOrDefault(t => t.Type == fileInfo.Type);

            if (target is null) throw new TeensyException($"Unsupported file type: {fileInfo.Type}");

            fileInfo.TargetPath = _settings.TargetRootPath
                .UnixPathCombine(target.TargetPath)
                .EnsureUnixPathEnding();
        }
    }
}