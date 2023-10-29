using System.Reactive;
using System.Reactive.Linq;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial.Services;
using TeensyRom.Core.Settings.Entities;
using TeensyRom.Core.Settings.Services;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Core.Storage.Extensions;
using TeensyRom.Core.Storage.Services;

namespace TeensyRom.Core.Storage
{
    /// <summary>
    /// Provides file operation capability.  Leverages the file watcher
    /// to automatically save new files to TeensyROM
    /// </summary>
    public class TeensyFileService: ITeensyFileService
    {
        private readonly ISettingsService _settingsService;
        private readonly IFileWatcher _fileWatcher;
        private readonly ITeensyObservableSerialPort _teensyPort;
        private readonly ILoggingService _logService;
        private IDisposable _settingsSubscription;
        private IDisposable _fileWatchSubscription;
        private TeensySettings _settings = new();

        public TeensyFileService(ISettingsService settingsService, IFileWatcher fileWatcher, ITeensyObservableSerialPort serialPort, ILoggingService logService)
        {
            _settingsService = settingsService;
            _fileWatcher = fileWatcher;
            _teensyPort = serialPort;
            _logService = logService;
            InitializeSettings();
        }

        private void InitializeSettings()
        {
            _settingsSubscription = _settingsService.Settings
                .Do(settings => _settings = settings)
                .Subscribe(settings => ToggleFileWatch(settings));
        }

        private void ToggleFileWatch(TeensySettings settings)
        {
            if(settings.AutoFileCopyEnabled is false)
            {
                _fileWatcher.Disable();
                return;
            }            
            _fileWatcher.Enable(
                settings.WatchDirectoryLocation,
                settings.FileTargets.Select(t => t.Extension).ToArray());

            _fileWatchSubscription ??= _fileWatcher.FileFound
                .Throttle(TimeSpan.FromMilliseconds(500))
                .WithLatestFrom(_teensyPort.IsConnected, (f, c) => new { File = f, IsConnected = c })
                .Where(fc => fc.IsConnected && settings.AutoFileCopyEnabled)
                .Select(fc => new TeensyFileInfo(fc.File))
                .Do(fileInfo => _logService.Log($"File detected: {fileInfo.FullPath}"))
                .Subscribe(fileInfo => SaveFile(fileInfo));
        }

        public Unit SaveFile(string path)
        {
            TeensyFileInfo fileInfo;

            try
            {                
                fileInfo = new TeensyFileInfo(path);
                SaveFile(fileInfo);
            }
            catch (FileNotFoundException ex)
            {
                _logService.Log(ex.Message);
            }
            return Unit.Default;
        }

        public Unit SaveFile(TeensyFileInfo fileInfo)
        {
            _logService.Log("Initiating file transfer handshake");            

            TransformDestination(fileInfo);

            if (_teensyPort.SendFile(fileInfo))
            {
                _logService.Log($"Saved: {fileInfo}");
            }
            else
            {
                _logService.Log($"Failed to save: {fileInfo}");
            }
            return Unit.Default;
        }

        private void TransformDestination(TeensyFileInfo fileInfo)
        {
            fileInfo.StorageType = _settings.TargetType;

            var target = _settings.FileTargets
                .FirstOrDefault(t => t.Type == fileInfo.Type);

            if(target is null)
            {
                throw new ArgumentException($"Unsupported file type: {fileInfo.Type}");
            }

            fileInfo.TargetPath = _settings.TargetRootPath
                .UnixPathCombine(target.TargetPath)
                .EnsureUnixPathEnding();
        }

        public void Dispose()
        {
            _settingsSubscription?.Dispose();
            _fileWatchSubscription?.Dispose();
            _fileWatcher.Dispose();
        }
    }
}