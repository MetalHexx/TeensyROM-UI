using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial.State;

namespace TeensyRom.Core.Serial.Commands.SaveFiles
{
    public static class SaveFilesSerialRoutine
    {
        private const int _retryLimit = 3;
        public static (List<FileTransferItem> SuccessfulFiles, List<FileTransferItem> FailedFiles) SaveFiles(this SerialPort serial, List<FileTransferItem> files, ILoggingService log)
        {
            List<FileTransferItem> successfulFiles = [];
            List<FileTransferItem> failedFiles = [];

            log.Internal($"Saving {files.Count} file(s) to the TR");

            var result = new SaveFilesResult();

            foreach (var file in files)
            {
                var success = CopyFile(serial, log, file, files);

                if (success)
                {
                    log.Internal($"Copying: {file.TargetPath.FileName} to {file.TargetPath.Directory}");
                    successfulFiles.Add(file);
                }
                else
                {
                    log.InternalError($"Failed to copy {file.TargetPath.FileName} after {_retryLimit} attempts");
                    failedFiles.Add(file);
                }
            }
            return (successfulFiles, failedFiles);
        }

        private static bool CopyFile(SerialPort serial, ILoggingService log, FileTransferItem file, List<FileTransferItem> files)
        {
            var retry = 0;

            while (retry < _retryLimit)
            {
                serial.ClearBuffers();
                try
                {
                    serial.SendIntBytes(TeensyToken.SendFile, 2);
                    serial.HandleAck();
                    serial.SendIntBytes(file.StreamLength, 4);
                    serial.SendIntBytes(file.Checksum, 2);
                    serial.SendIntBytes(file.TargetStorage.GetStorageToken(), 1);
                    serial.Write($"{file.TargetPath.Value}\0");
                    serial.HandleAck();
                    serial.ClearBuffers();

                    var bytesSent = 0;

                    while (file.StreamLength > bytesSent)
                    {
                        var bytesToSend = 16 * 1024;
                        if (file.StreamLength - bytesSent < bytesToSend) bytesToSend = (int)file.StreamLength - bytesSent;
                        serial.Write(file.Buffer, bytesSent, bytesToSend);

                        bytesSent += bytesToSend;
                    }
                    serial.HandleAck();
                    return true;
                }
                catch (Exception ex)
                {
                    retry++;
                    var response = serial.ReadSerialAsString(500);
                    var fileExistsMessage = "File already exists";

                    var isDuplicateFile = response.Contains(fileExistsMessage, StringComparison.OrdinalIgnoreCase)
                        || ex.Message.Contains(fileExistsMessage, StringComparison.OrdinalIgnoreCase);

                    if (isDuplicateFile)
                    {
                        log.InternalError($"Attempting to overwrite: {file.TargetPath.Value}");
                        TryDelete(serial, log, file);
                        continue;
                    }
                    log.InternalError($"Waiting {retry} seconds to retry.");
                    Thread.Sleep(1000 * retry);
                    log.InternalError($"Retry {retry} of {_retryLimit}");
                }
            }
            return false;
        }

        private static void TryDelete(SerialPort serial, ILoggingService log, FileTransferItem file)
        {
            try
            {
                serial.ClearBuffers();
                serial.SendIntBytes(TeensyToken.DeleteFile, 2);
                serial.HandleAck();
                serial.SendIntBytes(file.TargetStorage.GetStorageToken(), 1);
                serial.Write($"{file.TargetPath.Value}\0");
                serial.HandleAck();
                log.InternalSuccess($"Deleted file {file.TargetPath} successfully");
            }
            catch (Exception ex)
            {
                log.InternalError($"Error deleting file {file} \r\n => {ex.Message}");
            }
        }
    }
}
