using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Tools.D64Extraction;
using TeensyRom.Core.Storage.Tools.Zip;
using TeensyRom.Core.ValueObjects;

namespace TeensyRom.Core.Storage
{
    public interface IFileWatchService : IDisposable
    {
        IObservable<bool> IsProcessing { get; }

        void ToggleFileWatch(TeensySettings settings);
    }

    /// <summary>
    /// Provides file operation capability.  Leverages the file watcher
    /// to automatically save new files to TeensyROM
    /// </summary>
    public class FileWatchService : IFileWatchService
    {
        public IObservable<bool> IsProcessing => _isProcessing.AsObservable();
        private readonly BehaviorSubject<bool> _isProcessing = new(false);

        private readonly string _assemblyDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!;
        private readonly IFileTransferService _fileTransfer;
        private readonly ISettingsService _settingsService;
        private readonly IFileWatcher _fileWatcher;
        private readonly ISerialStateContext _serialState;
        private readonly ILoggingService _log;
        private readonly IAlertService _alert;
        private TeensySettings _settings = null!;
        private IDisposable? _settingsSubscription = null!;
        private IDisposable? _fileWatchSubscription = null!;

        public FileWatchService(IFileTransferService fileTransfer, ISettingsService settingsService, IFileWatcher fileWatcher, ISerialStateContext serialState, ILoggingService log, IAlertService alert)
        {
            _fileTransfer = fileTransfer;
            _settingsService = settingsService;
            _fileWatcher = fileWatcher;
            _serialState = serialState;
            _log = log;
            _alert = alert;
            _settingsSubscription = _settingsService.Settings
                .Where(s => s is not null)
                .Select(s => s with { })
                .Subscribe(ToggleFileWatch);
        }

        public void ToggleFileWatch(TeensySettings settings)
        {
            _settings = settings;

            if (_settings.AutoFileCopyEnabled is false)
            {
                _fileWatcher.Disable();
                return;
            }
            var typesToWatch = _settings.FileTargets
                .Select(t => t.Extension)
                .ToList();

            typesToWatch.Add(".zip");

            _fileWatcher.Enable(
                _settings.WatchDirectoryLocation,
                typesToWatch.ToArray());

            _fileWatchSubscription ??= _fileWatcher.FilesFound
                .Where(NotInAssemblyDirectory)
                .CombineLatest(_serialState.CurrentState, (files, serialState) => (files, serialState))
                .Where(fileSerial => fileSerial.serialState is SerialConnectedState && _settings.AutoFileCopyEnabled)
                .Select(fileSerial => fileSerial.files)
                .DistinctUntilChanged()
                .SubscribeOn(TaskPoolScheduler.Default)
                .Select(files => files.Select(f => new FileTransferItem
                (
                    fileInfo: f,
                    targetFilePath: GetTargetPath(f),
                    targetStorage: _settings.StorageType
                )))
                .ObserveOn(Scheduler.Default)
                .Where(fti => fti.Any())
                .Subscribe(fti =>
                {
                    int fileCount = fti.Count();
                    
                    _log.Internal($"Starting auto transfer of {fileCount} files.");
                    _alert.Publish($"Starting auto transfer of {fileCount} files.");
                    
                    _isProcessing.OnNext(true);
                    _fileTransfer.Send(fti.ToList());
                    _isProcessing.OnNext(false);
                });
        }

        private bool NotInAssemblyDirectory(List<FileInfo> files)
        {
            return !files.Any(f => f.DirectoryName is not null && f.DirectoryName.Contains(_assemblyDirectory));
        }

        private FilePath GetTargetPath(FileInfo file)
        {
            var path = new FilePath(file.FullName
                .Replace(_settings.WatchDirectoryLocation, string.Empty)
                .ToUnixPath());

            if (file.Extension.GetFileType() is TeensyFileType.Hex) 
            {
                return new DirectoryPath(StorageHelper.Firmware_Path).Combine(path);
            }
            return _settings.AutoTransferPath.Combine(path);
        }

        public void Dispose()
        {
            _settingsSubscription?.Dispose();
            _fileWatchSubscription?.Dispose();
            _fileWatcher?.Dispose();
        }
    }
}