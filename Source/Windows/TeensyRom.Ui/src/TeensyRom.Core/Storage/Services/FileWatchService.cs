using MediatR;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime;
using System.Runtime.CompilerServices;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Common;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Core.Storage.Services;
using TeensyRom.Core.Storage.Tools.D64Extraction;
using TeensyRom.Core.Storage.Tools.Zip;

namespace TeensyRom.Core.Storage
{
    public interface IFileWatchService : IDisposable 
    {
        IObservable<bool> IsProcessing { get; }
        IObservable<List<FileTransferItem>> WatchFiles { get; }
        void ToggleFileWatch(TeensySettings settings);
    }

    /// <summary>
    /// Provides file operation capability.  Leverages the file watcher
    /// to automatically save new files to TeensyROM
    /// </summary>
    public class FileWatchService: IFileWatchService
    {
        public IObservable<List<FileTransferItem>> WatchFiles => _watchFiles.AsObservable();
        private readonly Subject<List<FileTransferItem>> _watchFiles = new();

        public IObservable<bool> IsProcessing => _isProcessing.AsObservable();
        private readonly BehaviorSubject<bool> _isProcessing = new(false);
        

        private readonly ISettingsService _settingsService;
        private readonly IFileWatcher _fileWatcher;
        private readonly ISerialStateContext _serialState;
        private readonly ID64Extractor _d64Extractor;
        private readonly IZipExtractor _zipExtractor;
        private readonly ILoggingService _log;
        private readonly IAlertService _alert;
        private TeensySettings _settings = null!;
        private IDisposable? _settingsSubscription = null!;
        private IDisposable? _fileWatchSubscription = null!;

        public FileWatchService(ISettingsService settingsService, IFileWatcher fileWatcher, ISerialStateContext serialState, ID64Extractor d64Extractor, IZipExtractor zipExtractor,  ILoggingService log, IAlertService alert)
        {
            _settingsService = settingsService;
            _fileWatcher = fileWatcher;
            _serialState = serialState;
            _d64Extractor = d64Extractor;
            _zipExtractor = zipExtractor;
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
                .CombineLatest(_serialState.CurrentState, (file, serialState) => (file, serialState))
                .Where(fileSerial => fileSerial.serialState is SerialConnectedState && _settings.AutoFileCopyEnabled)
                .Select(fileSerial => fileSerial.file)
                .DistinctUntilChanged()
                .SubscribeOn(TaskPoolScheduler.Default)
                .Select(files => files.Select(f => new FileTransferItem
                (
                    fileInfo: f,
                    targetPath: _settings.GetAutoTransferPath(f.Extension.GetFileType()),
                    targetStorage: _settings.StorageType
                )))
                .ObserveOn(Scheduler.Default)
                .Select(x => Extract(x))                
                .Subscribe(fti => _watchFiles.OnNext(fti.ToList()));
        }

        private IEnumerable<FileTransferItem> Extract(IEnumerable<FileTransferItem> files)        
        {
            var transferItems = ExtractZip(files);
            transferItems = ExtractD64(transferItems);            
            return transferItems;
        }

        private IEnumerable<FileTransferItem> ExtractZip(IEnumerable<FileTransferItem> files) 
        {
            var zipFiles = files.Where(file => new FileInfo(file.SourcePath).Extension.Contains("zip", StringComparison.OrdinalIgnoreCase));
            
            if (!zipFiles.Any())
            {
                return files;
            }
            var fileMessageString = zipFiles.Count() > 1 ? "files" : "file";
            _alert.Publish($"Unpacking ZIP {fileMessageString}.");

            var extractedFiles = zipFiles
                .Select(f => _zipExtractor.Extract(f))
                .Select(f => f.ExtractedFiles
                    .Select(fileInfo => new FileTransferItem
                    (
                        sourcePath: fileInfo.FullName,
                        targetPath: _settings.GetAutoTransferPath(fileInfo.Extension.GetFileType()),
                        targetStorage: _settings.StorageType)
                    ))
                .SelectMany(f => f)
                .Where(f => f.Type != TeensyFileType.Unknown)
                .ToList();

            if(!extractedFiles.Any())
            {
                _alert.Publish($"No compatible files found in ZIP {fileMessageString}.");
            }

            var finalList = files
                .Where(f => !zipFiles.Any(zip => f.Name == zip.Name))
                .ToList();

            finalList.AddRange(extractedFiles);

            return finalList;
        }

        private IEnumerable<FileTransferItem> ExtractD64(IEnumerable<FileTransferItem> files)
        {
            if (!files.Any(files => files.Type == TeensyFileType.D64))
            {
                return files;
            }
            _alert.Publish("D64 files detected.  Unpacking PRGs.");
            var extractedPrgs = files
                .Where(f => f.Type == TeensyFileType.D64)                
                .Select(f => _d64Extractor.Extract(f))
                .Select(f => f.ExtractedFiles
                    .Select(prgInfo => new FileTransferItem
                    (
                        sourcePath: prgInfo.FullName,
                        targetPath: _settings.GetAutoTransferPath(prgInfo.Extension.GetFileType()).UnixPathCombine(f.OriginalFileName),
                        targetStorage: _settings.StorageType)
                    ))
                .SelectMany(f => f)
                .ToList();

            var finalList = files
                .Where(f => f.Type is not TeensyFileType.D64)
                .ToList();

            finalList.AddRange(extractedPrgs);

            return finalList;
        }

        public void Dispose()
        {
            _settingsSubscription?.Dispose();
            _fileWatchSubscription?.Dispose();
            _fileWatcher?.Dispose();
        }
    }
}