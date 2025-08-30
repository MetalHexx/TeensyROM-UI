using MediatR;
using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;

namespace TeensyRom.Core.Commands
{
    public class SaveFilesCommandHandler(ILoggingService logService) : IRequestHandler<SaveFilesCommand, SaveFilesResult>
    {
        private ISerialStateContext _serialState = null!;
        private readonly int _retryLimit = 3;

        public Task<SaveFilesResult> Handle(SaveFilesCommand command, CancellationToken ct)
        {
            _serialState = command.Serial;

            logService.Internal($"Saving {command.Files.Count} file(s) to the TR");

            var result = new SaveFilesResult();

            foreach (var file in command.Files)
            {
                logService.Internal($"Saving File: {file.TargetPath.Value}");

                var success = SaveFile(file, command);

                if (success)
                {
                    logService.InternalSuccess($"Save Success: {file.TargetPath.Value}");
                    result.SuccessfulFiles.Add(file);
                }
                else
                {
                    logService.InternalError($"Save Failed: Failed to copy {file.TargetPath.FileName} after {_retryLimit} attempts");
                    result.FailedFiles.Add(file);
                }
            }
            return Task.FromResult(result);
        }

        private bool SaveFile(FileTransferItem file, SaveFilesCommand command) 
        {
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
                    _serialState.SendIntBytes(file.TargetStorage.GetStorageToken(), 1);
                    _serialState.Write($"{file.TargetPath.Value}\0");
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
                    var fileExistsParseMessage = "File already exists";

                    var fileExists = response.Contains(fileExistsParseMessage, StringComparison.OrdinalIgnoreCase)
                        || ex.Message.Contains(fileExistsParseMessage, StringComparison.OrdinalIgnoreCase);

                    if (fileExists)
                    {
                        logService.InternalError($"Save Error: {file.TargetPath.Value} already exists on TR.");
                        logService.Internal($"Delete Attempt: {file.TargetPath.Value}");
                        TryDelete(file);
                        continue;
                    }
                    logService.InternalError($"Save Retry: {retry} seconds to retry.");
                    Thread.Sleep(1000 * retry);
                    logService.InternalError($"Save Retry: {retry} of {_retryLimit}");
                }
            }
            return false;
        }

        private void TryDelete(FileTransferItem file) 
        {
            try
            {
                _serialState.ClearBuffers();
                _serialState.SendIntBytes(TeensyToken.DeleteFile, 2);
                _serialState.HandleAck();
                _serialState.SendIntBytes(file.TargetStorage.GetStorageToken(), 1);
                _serialState.Write($"{file.TargetPath.Value}\0");
                _serialState.HandleAck();
                logService.InternalSuccess($"Delete Success: {file.TargetPath}");
            }
            catch (Exception ex)
            {
                logService.InternalError($"Delete Error: {file} \r\n => {ex.Message}");
            }
        }
    }
}