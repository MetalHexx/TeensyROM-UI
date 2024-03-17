using MediatR;
using System.Reactive.Linq;
using TeensyRom.Core.Common;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands
{
    public class QueuedSaveFileCommandHandler : IRequestHandler<QueuedSaveFileCommand, QueuedSaveFileResult>
    {
        private TeensySettings _settings = null!;
        private readonly ISerialStateContext _serialState;

        public QueuedSaveFileCommandHandler(ISerialStateContext serialState, ISettingsService settings)
        {
            settings.Settings.Take(1).Subscribe(s => _settings = s);
            _serialState = serialState;
        }

        public Task<QueuedSaveFileResult> Handle(QueuedSaveFileCommand r, CancellationToken cancellationToken)
        {
            TransformDestination(r.File);

            _serialState.SendIntBytes(TeensyToken.SendFile, 2);

            _serialState.WaitForSerialData(numBytes: 2, timeoutMs: 500);

            _serialState.HandleAck();
            _serialState.SendIntBytes(r.File.StreamLength, 4);
            _serialState.SendIntBytes(r.File.Checksum, 2);
            _serialState.SendIntBytes(r.File.StorageType.GetStorageToken(), 1);
            _serialState.Write($"{r.File.TargetPath.UnixPathCombine(r.File.Name)}\0");
            _serialState.HandleAck();

            var bytesSent = 0;

            while (r.File.StreamLength > bytesSent)
            {
                var bytesToSend = 16 * 1024;
                if (r.File.StreamLength - bytesSent < bytesToSend) bytesToSend = (int)r.File.StreamLength - bytesSent;
                _serialState.Write(r.File.Buffer, bytesSent, bytesToSend);

                bytesSent += bytesToSend;
            }
            _serialState.HandleAck();
            return Task.FromResult(new QueuedSaveFileResult());
        }

        private void TransformDestination(TeensyFileInfo fileInfo)
        {
            if(!string.IsNullOrWhiteSpace(fileInfo.TargetPath)) return;

            fileInfo.StorageType = _settings.TargetType;
            fileInfo.TargetPath = _settings.GetAutoTransferPath(fileInfo.Type);
        }
    }
}