using MediatR;
using System.Reactive.Linq;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Core.Storage.Services;

namespace TeensyRom.Core.Storage
{
    public interface IFileWatchService : IDisposable 
    {
        void ToggleFileWatch(TeensySettings settings);
    }

    /// <summary>
    /// Provides file operation capability.  Leverages the file watcher
    /// to automatically save new files to TeensyROM
    /// </summary>
    public class FileWatchService: IFileWatchService
    {
        private readonly ISettingsService _settingsService;
        private readonly IFileWatcher _fileWatcher;
        private readonly ISerialPortState _serialState
            ;
        private readonly ILoggingService _logService;
        private readonly ICachedStorageService _storageService;
        private readonly IMediator _mediator;
        private IDisposable _settingsSubscription;
        private IDisposable _fileWatchSubscription;

        public FileWatchService(ISettingsService settingsService, IFileWatcher fileWatcher, ISerialPortState serialState, ILoggingService logService, ICachedStorageService storageService)
        {
            _settingsService = settingsService;
            _fileWatcher = fileWatcher;
            _serialState = serialState;
            _logService = logService;
            _storageService = storageService;

            _settingsSubscription = _settingsService.Settings
                .Subscribe(settings => ToggleFileWatch(settings));
        }

        public void ToggleFileWatch(TeensySettings settings)
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
                .WithLatestFrom(_serialState.IsConnected, (f, c) => new { File = f, IsConnected = c })
                .Where(fc => fc.IsConnected && settings.AutoFileCopyEnabled)
                .Select(fc => new TeensyFileInfo(fc.File))
                .Do(fileInfo => _logService.Internal($"File detected: {fileInfo.FullPath}"))
                .Subscribe(async fileInfo => await _storageService.QueuedSaveFile(fileInfo));
        }

        public void Dispose()
        {
            _settingsSubscription?.Dispose();
            _fileWatchSubscription?.Dispose();
            _fileWatcher.Dispose();
        }
    }
}