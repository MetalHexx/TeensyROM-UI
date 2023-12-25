using System.Reactive;
using TeensyRom.Core.Common;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands.File.LaunchFile
{
    public interface ILaunchFileCommand
    {
        bool Execute(string path);
        bool LaunchFile(LaunchFileRequest request);
    }

    public class LaunchFileCommand : TeensyCommand, ILaunchFileCommand
    {
        private readonly IResetCommand _resetCommand;

        public LaunchFileCommand(ISettingsService settingsService, IObservableSerialPort serialPort, ILoggingService logService, IResetCommand resetCommand) : base(settingsService, serialPort, logService)
        {
            _resetCommand = resetCommand;
        }

        public bool Execute(string path)
        {
            var request = new LaunchFileRequest
            {
                StorageType = _settings.TargetType,
                TargetPath = path
            };
            if (LaunchFile(request))
            {
                _logService.Log($"Launched: {path}");
                return true;
            }
            _logService.Log($"Failed to launch: {path}");

            return false;
        }

        public bool LaunchFile(LaunchFileRequest request)
        {
            _serialPort.DisableAutoReadStream();

            try
            {
                _logService.Log($"Sending launch file token: {TeensyConstants.Launch_File_Token}");
                _serialPort.SendIntBytes(TeensyConstants.Launch_File_Token, 2);

                if (!GetAck())
                {
                    ReadSerialAsString();
                    throw new TeensyException("Error getting acknowledgement when Launch File Token sent");
                }

                _logService.Log($"Sending SD_nUSB: {TeensyConstants.Sd_Card_Token}");
                _serialPort.SendIntBytes(GetStorageToken(request.StorageType), 1);

                _logService.Log($"Sending file launch request path: {request.TargetPath}");
                _serialPort.Write($"{request.TargetPath}\0");

                if (!GetAck())
                {
                    ReadSerialAsString(msToWait: 100);
                    throw new TeensyException("Error getting acknowledgement when launch path sent");
                }
                _logService.Log("Launch file request complete!");
            }
            catch (TeensyException ex)
            {
                _logService.Log($"Error launching file.  {ex.Message}");
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
