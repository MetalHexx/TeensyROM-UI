using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Common;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands
{
    public interface ICopyFileCommand
    {
        bool Execute(string sourcePath, string destPath);
    }

    public class CopyFileCommand : TeensyCommand, ICopyFileCommand
    {
        public CopyFileCommand(ISettingsService settingsService, IObservableSerialPort serialPort, ILoggingService logService)
            : base(settingsService, serialPort, logService) { }

        public bool Execute(string sourcePath, string destPath)
        {
            _logService.Log("Initiating file copy handshake");

            if (SendFile(sourcePath, destPath))
            {
                _logService.Log($"Copied: {sourcePath} to {destPath}");
                return true;
            }
            else
            {
                _logService.Log($"Failed to copy: {sourcePath} to {destPath}");
                return false;
            }
        }

        public bool SendFile(string sourcePath, string destPath)
        {
            _serialPort.DisableAutoReadStream();

            try
            {
                _logService.Log($"Sending copy file token: {TeensyConstants.Copy_File_Token}");
                _serialPort.SendIntBytes(TeensyConstants.Copy_File_Token, 2);

                _logService.Log($"Sending SD_nUSB: {TeensyConstants.Sd_Card_Token}");
                _serialPort.SendIntBytes(GetStorageToken(_settings.TargetType), 1);

                _logService.Log($"Sending source path: {sourcePath}");
                _serialPort.Write($"{sourcePath}\0");

                _logService.Log($"Sending destination path: {destPath}");
                _serialPort.Write($"{destPath}\0");

                if (!GetAck())
                {
                    ReadSerialAsString(msToWait: 100);
                    throw new TeensyException("Error getting acknowledgement of successful file copy");
                }               
                _logService.Log("File transfer complete!");
            }
            catch (Exception ex)
            {
                _logService.Log($"Got an exception trying to perform the file copy operation: {ex}");
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
