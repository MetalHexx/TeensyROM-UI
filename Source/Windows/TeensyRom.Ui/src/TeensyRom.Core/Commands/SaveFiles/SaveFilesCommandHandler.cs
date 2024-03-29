using MediatR;
using System.Reactive.Linq;
using TeensyRom.Core.Common;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands
{
    public class SaveFilesCommandHandler : IRequestHandler<SaveFilesCommand, SaveFilesResult>
    {
        private TeensySettings _settings = null!;
        private readonly ISerialStateContext _serialState;
        private readonly ILoggingService _logService;
        private int _retryLimit = 3;

        public SaveFilesCommandHandler(ISerialStateContext serialState, ISettingsService settings, ILoggingService logService)
        {
            settings.Settings.Take(1).Subscribe(s => _settings = s);
            _serialState = serialState;
            _logService = logService;
        }

        public Task<SaveFilesResult> Handle(SaveFilesCommand r, CancellationToken ct)
        {
            _logService.Internal($"Saving {r.Files.Count} file(s) to the TR");

            var result = new SaveFilesResult();

            foreach (var file in r.Files)
            {
                var success = CopyFile(file);

                if (success)
                {
                    _logService.Internal($"Copying: {file.TargetPath.UnixPathCombine(file.Name)}");
                    result.SuccessfulFiles.Add(file);
                }
                else
                {
                    _logService.InternalError($"Failed to copy {file.Name} after {_retryLimit} attempts");
                    result.FailedFiles.Add(file);
                }
            }
            return Task.FromResult(result);
        }

        private bool CopyFile(TeensyFileInfo file) 
        {
            TransformDestination(file);
            var retry = 0;

            while (retry < _retryLimit)
            {
                _serialState.ClearBuffers();
                try
                {
                    _serialState.SendIntBytes(TeensyToken.SendFile, 2);
                    _serialState.HandleAck();
                    _serialState.SendIntBytes(file.StreamLength, 4);
                    _serialState.SendIntBytes(file.Checksum, 2);
                    _serialState.SendIntBytes(file.StorageType.GetStorageToken(), 1);
                    _serialState.Write($"{file.TargetPath.UnixPathCombine(file.Name)}\0");
                    _serialState.HandleAck();
                    _serialState.ClearBuffers();

                    var bytesSent = 0;

                    while (file.StreamLength > bytesSent)
                    {
                        var bytesToSend = 16 * 1024;
                        if (file.StreamLength - bytesSent < bytesToSend) bytesToSend = (int)file.StreamLength - bytesSent;
                        _serialState.Write(file.Buffer, bytesSent, bytesToSend);

                        bytesSent += bytesToSend;
                    }
                    _serialState.HandleAck();
                    return true;
                }
                catch (Exception ex)
                {
                    retry++;
                    var response = _serialState.ReadSerialAsString(500);
                    var fileExistsMessage = "File already exists";

                    var isDuplicateFile = response.Contains(fileExistsMessage, StringComparison.OrdinalIgnoreCase)
                        || ex.Message.Contains(fileExistsMessage, StringComparison.OrdinalIgnoreCase);

                    if (isDuplicateFile)
                    {
                        _logService.InternalError($"Attempting to overwrite: {file.TargetPath.UnixPathCombine(file.Name)}");
                        TryDelete($"{file.TargetPath.UnixPathCombine(file.Name)}");
                        continue;
                    }
                    _logService.InternalError($"Waiting {retry} seconds to retry.");
                    Thread.Sleep(1000 * retry);
                    _logService.InternalError($"Retry {retry} of {_retryLimit}");
                }
            }
            return false;
        }

        private void TryDelete(string path) 
        {
            try
            {
                _serialState.ClearBuffers();
                _serialState.SendIntBytes(TeensyToken.DeleteFile, 2);
                _serialState.HandleAck();
                _serialState.SendIntBytes(_settings.TargetType.GetStorageToken(), 1);
                _serialState.Write($"{path}\0");
                _serialState.HandleAck();
                _logService.InternalSuccess($"Deleted file {path} successfully");
            }
            catch (Exception ex)
            {
                _logService.InternalError($"Error deleting file {path} \r\n => {ex.Message}");
            }
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