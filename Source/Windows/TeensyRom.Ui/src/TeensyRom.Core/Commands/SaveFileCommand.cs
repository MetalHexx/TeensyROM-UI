using System.Reactive;
using TeensyRom.Core.Common;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands
{
    public interface ISaveFileCommand
    {
        Unit Execute(TeensyFileInfo file);
    }
    public class SaveFileCommand : TeensyCommand, ISaveFileCommand
    {
        public SaveFileCommand(ISettingsService settingsService, IObservableSerialPort serialPort, ILoggingService logService)
            : base(settingsService, serialPort, logService) { }

        public Unit Execute(TeensyFileInfo file)
        {
            _logService.Log("Initiating file transfer handshake");

            TransformDestination(file);

            if (SendFile(file))
            {
                _logService.Log($"Saved: {file}");
            }
            else
            {
                _logService.Log($"Failed to save: {file}");
            }
            return Unit.Default;
        }

        private void TransformDestination(TeensyFileInfo fileInfo)
        {
            fileInfo.StorageType = _settings.TargetType;

            var target = _settings.FileTargets
                .FirstOrDefault(t => t.Type == fileInfo.Type);

            if (target is null)
            {
                throw new ArgumentException($"Unsupported file type: {fileInfo.Type}");
            }

            fileInfo.TargetPath = _settings.TargetRootPath
                .UnixPathCombine(target.TargetPath)
                .EnsureUnixPathEnding();
        }

        public bool SendFile(TeensyFileInfo fileInfo)
        {
            _serialPort.DisableAutoReadStream();

            try
            {
                _logService.Log($"Sending file transfer token: {TeensyConstants.Send_File_Token}");
                _serialPort.SendIntBytes(TeensyConstants.Send_File_Token, 2);

                WaitForSerialData(numBytes: 2, timeoutMs: 500);

                if (!GetAck())
                {
                    ReadSerialAsString();
                    throw new TeensyException("Error getting acknowledgement when Send File Token sent");
                }

                _logService.Log($"Sending Stream Length: {fileInfo.StreamLength}");
                _serialPort.SendIntBytes(fileInfo.StreamLength, 4);

                _logService.Log($"Sending Checksum: {fileInfo.Checksum}");
                _serialPort.SendIntBytes(fileInfo.Checksum, 2);

                _logService.Log($"Sending SD_nUSB: {TeensyConstants.Sd_Card_Token}");
                _serialPort.SendIntBytes(GetStorageToken(fileInfo.StorageType), 1);

                _logService.Log($"Sending to target path: {fileInfo.TargetPath.UnixPathCombine(fileInfo.Name)}");
                _serialPort.Write($"{fileInfo.TargetPath.UnixPathCombine(fileInfo.Name)}\0");

                if (!GetAck())
                {
                    ReadSerialAsString(msToWait: 100);
                    throw new TeensyException("Error getting acknowledgement when file metadata sent");
                }
                _logService.Log("File ready for transfer!");

                _logService.Log($"Sending file: {fileInfo.FullPath}");
                var bytesSent = 0;

                while (fileInfo.StreamLength > bytesSent)
                {
                    var bytesToSend = 16 * 1024;
                    if (fileInfo.StreamLength - bytesSent < bytesToSend) bytesToSend = (int)fileInfo.StreamLength - bytesSent;
                    _serialPort.Write(fileInfo.Buffer, bytesSent, bytesToSend);

                    _logService.Log("*");
                    bytesSent += bytesToSend;
                }

                if (!GetAck())
                {
                    ReadSerialAsString(msToWait: 500);
                    _logService.Log("File transfer failed.");
                    throw new TeensyException("Error getting acknowledgement when sending file");
                }
                _logService.Log("File transfer complete!");
            }
            catch (TeensyException)
            {
                return false;
            }
            finally
            {
                _serialPort.EnableAutoReadStream();
            }
            return true;
        }
    }
}
