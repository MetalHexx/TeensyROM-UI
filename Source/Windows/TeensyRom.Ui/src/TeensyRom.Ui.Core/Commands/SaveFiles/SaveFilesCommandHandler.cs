using MediatR;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Common;
using TeensyRom.Ui.Core.Serial;
using TeensyRom.Ui.Core.Serial.State;
using TeensyRom.Ui.Core.Storage.Entities;

namespace TeensyRom.Ui.Core.Commands
{
    public class SaveFilesCommandHandler : IRequestHandler<SaveFilesCommand, SaveFilesResult>
    {
        private readonly ISerialStateContext _serialState;
        private readonly ILoggingService _logService;
        private readonly int _retryLimit = 3;

        public SaveFilesCommandHandler(ISerialStateContext serialState, ILoggingService logService)
        {
            _serialState = serialState;
            _logService = logService;
        }

        public Task<SaveFilesResult> Handle(SaveFilesCommand command, CancellationToken ct)
        {
            _logService.Internal($"Saving {command.Files.Count} file(s) to the TR");

            var result = new SaveFilesResult();

            foreach (var file in command.Files)
            {
                var success = CopyFile(file, command);

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
                        _logService.InternalError($"Attempting to overwrite: {file.TargetPath.UnixPathCombine(file.Name)}");
                        TryDelete(file);
                        continue;
                    }
                    _logService.InternalError($"Waiting {retry} seconds to retry.");
                    Thread.Sleep(1000 * retry);
                    _logService.InternalError($"Retry {retry} of {_retryLimit}");
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
                _logService.InternalSuccess($"Deleted file {file} successfully");
            }
            catch (Exception ex)
            {
                _logService.InternalError($"Error deleting file {file} \r\n => {ex.Message}");
            }
        }
    }
}