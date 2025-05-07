using MediatR;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.State;

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
                var success = CopyFile(file, command);

                if (success)
                {
                    logService.Internal($"Copying: {file.TargetPath.UnixPathCombine(file.Name)}");
                    result.SuccessfulFiles.Add(file);
                }
                else
                {
                    logService.InternalError($"Failed to copy {file.Name} after {_retryLimit} attempts");
                    result.FailedFiles.Add(file);
                }
            }
            return Task.FromResult(result);
        }

        private bool CopyFile(FileTransferItem file, SaveFilesCommand command) 
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
                        logService.InternalError($"Attempting to overwrite: {file.TargetPath.UnixPathCombine(file.Name)}");
                        TryDelete(file);
                        continue;
                    }
                    logService.InternalError($"Waiting {retry} seconds to retry.");
                    Thread.Sleep(1000 * retry);
                    logService.InternalError($"Retry {retry} of {_retryLimit}");
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
                _serialState.Write($"{file.TargetPath.UnixPathCombine(file.Name)}\0");
                _serialState.HandleAck();
                logService.InternalSuccess($"Deleted file {file.TargetPath} successfully");
            }
            catch (Exception ex)
            {
                logService.InternalError($"Error deleting file {file} \r\n => {ex.Message}");
            }
        }
    }
}