using System;
using System.Collections.Concurrent;
using System.Data;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using TeensyRom.Core.Logging;

namespace TeensyRom.Core.Storage
{
    public class FileWatcher : IFileWatcher, IDisposable
    {
        private readonly Subject<List<FileInfo>> _filesFound = new();
        private readonly ILoggingService _logger;
        private readonly IAlertService _alert;

        public IObservable<List<FileInfo>> FilesFound => _filesFound.AsObservable();

        private FileSystemWatcher? _watcher = null;
        private string[] _fileTypes = Array.Empty<string>();
        private IDisposable? _watchSubscription;

        // For tracking when a new batch of files starts being detected
        private DateTime _lastBatchTime = DateTime.MinValue;
        private const int NEW_BATCH_THRESHOLD_MS = 5000; // 10 seconds

        public FileWatcher(ILoggingService log, IAlertService alert)
        {
            _logger = log;
            _alert = alert;
        }

        public void Disable()
        {
            _watchSubscription?.Dispose();
            _watcher?.Dispose();
            _watchSubscription = null;
        }

        public void Enable(string path, params string[] fileTypes)
        {
            _fileTypes = fileTypes;
            _lastBatchTime = DateTime.MinValue;

            _watchSubscription?.Dispose();
            _watcher?.Dispose();
            _watcher = new()
            {
                Path = path,
                NotifyFilter = NotifyFilters.LastWrite
                 | NotifyFilters.FileName
                 | NotifyFilters.DirectoryName
                 | NotifyFilters.CreationTime
                 | NotifyFilters.Size
                 | NotifyFilters.Attributes,
                Filter = "*.*",
                EnableRaisingEvents = true,
                InternalBufferSize = 65536,
                IncludeSubdirectories = true
            };
            _watchSubscription = InitializeWatcher();
        }

        private IDisposable InitializeWatcher()
        {
            if (_watcher is null)
                throw new InvalidOperationException("Watcher not initialized");

            var createdEvents = Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                    handler => _watcher.Created += handler,
                    handler => _watcher.Created -= handler)
                    .Select(evt => evt.EventArgs.FullPath);

            var changedEvents = Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                    handler => _watcher.Changed += handler,
                    handler => _watcher.Changed -= handler)
                    .Select(evt => evt.EventArgs.FullPath);

            var renamedEvents = Observable.FromEventPattern<RenamedEventHandler, RenamedEventArgs>(
                    handler => _watcher.Renamed += handler,
                    handler => _watcher.Renamed -= handler)
                    .Select(evt => evt.EventArgs.FullPath);

            // Create a merged stream of all events
            var allFileEvents = createdEvents
                .Merge(changedEvents)
                .Merge(renamedEvents)
                .Where(File.Exists)
                .Select(path => new FileInfo(path))
                .Where(AcceptedType)
                .Where(_ => _watchSubscription is not null);

            // Add a listener to provide early notification for each new batch
            allFileEvents.Subscribe(file => {
                var now = DateTime.UtcNow;
                
                // If it's been a while since we detected files, this is likely a new batch
                if ((now - _lastBatchTime).TotalMilliseconds > NEW_BATCH_THRESHOLD_MS)
                {
                    _logger.Internal("Files detected. Preparing to process files for transfer...");
                    _alert.Publish("Files detected. Preparing to process files for transfer...");
                }
                
                // Update the last batch time
                _lastBatchTime = now;
            });

            // The actual file buffering remains unchanged
            return allFileEvents
                .Publish(stream => stream
                    .Buffer(() => stream.Throttle(TimeSpan.FromSeconds(5)))
                    .Select(buffered => buffered
                        .DistinctBy(file => file.FullName)
                        .ToList()))
                .Subscribe(_filesFound.OnNext);
        }

        private static List<string> GetFilesRecursive(string path)
        {
            if (File.Exists(path))
            {
                return
                [
                    path
                ];
            }
            else if (Directory.Exists(path))
            {
                var files = Directory
                    .GetFiles(path)
                    .ToList();

                var deeperFiles = Directory.GetDirectories(path)
                    .SelectMany(GetFilesRecursive);

                return [.. files, .. deeperFiles];
            }
            return [];
        }

        private bool AcceptedType(FileInfo fileInfo) => _fileTypes.Any(t => t.Equals(fileInfo.Extension, StringComparison.OrdinalIgnoreCase));
        
        public void Dispose()
        {
            _watchSubscription?.Dispose();
            _watcher?.Dispose();
        }
    }
}