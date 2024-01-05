using MediatR;
using System.Reactive.Linq;
using TeensyRom.Core.Common;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands
{
    public class SaveFileCommandHandler : IRequestHandler<SaveFileCommand, SaveFileResult>
    {
        private TeensySettings _settings;
        private readonly IObservableSerialPort _serialPort;

        public SaveFileCommandHandler(IObservableSerialPort serialPort, ISettingsService settings)
        {
            settings.Settings.Take(1).Subscribe(s => _settings = s);
            _serialPort = serialPort;
        }

        public Task<SaveFileResult> Handle(SaveFileCommand r, CancellationToken cancellationToken)
        {
            TransformDestination(r.File);

            _serialPort.SendIntBytes(TeensyToken.SendFile, 2);

            _serialPort.WaitForSerialData(numBytes: 2, timeoutMs: 500);

            _serialPort.HandleAck();
            _serialPort.SendIntBytes(r.File.StreamLength, 4);
            _serialPort.SendIntBytes(r.File.Checksum, 2);
            _serialPort.SendIntBytes(r.File.StorageType.GetStorageToken(), 1);
            _serialPort.Write($"{r.File.TargetPath.UnixPathCombine(r.File.Name)}\0");
            _serialPort.HandleAck();

            var bytesSent = 0;

            while (r.File.StreamLength > bytesSent)
            {
                var bytesToSend = 16 * 1024;
                if (r.File.StreamLength - bytesSent < bytesToSend) bytesToSend = (int)r.File.StreamLength - bytesSent;
                _serialPort.Write(r.File.Buffer, bytesSent, bytesToSend);

                bytesSent += bytesToSend;
            }
            _serialPort.HandleAck();
            return Task.FromResult(new SaveFileResult());
        }

        private void TransformDestination(TeensyFileInfo fileInfo)
        {
            if(!string.IsNullOrWhiteSpace(fileInfo.TargetPath)) return;

            fileInfo.StorageType = _settings.TargetType;

            var target = _settings.FileTargets
                .FirstOrDefault(t => t.Type == fileInfo.Type);

            if (target is null) throw new TeensyException($"Unsupported file type: {fileInfo.Type}");

            fileInfo.TargetPath = _settings.TargetRootPath
                .UnixPathCombine(_settings.GetFileTypePath(fileInfo.Type), _settings.AutoTransferPath)
                .EnsureUnixPathEnding();
        }
    }
}