using System;
using System.Collections.Concurrent;
using System.Data;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using TeensyRom.Core.Logging;

namespace TeensyRom.Core.Storage
{

    public class FileWatcher : IFileWatcher, IDisposable
    {
        private readonly Subject<List<FileInfo>> _fileFound = new();
        public IObservable<List<FileInfo>> FilesFound => _fileFound.AsObservable();

        private FileSystemWatcher? _watcher = null;
        private string[] _fileTypes = Array.Empty<string>();
        private IDisposable? _watchSubscription;

        public void Disable()
        {
            _watchSubscription?.Dispose();
            _watcher?.Dispose();
            _watchSubscription = null;
        }

        public void Enable(string path, params string[] fileTypes)
        {
            _fileTypes = fileTypes;

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

            var changedEvents = Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                    handler => _watcher.Changed += handler,
                    handler => _watcher.Changed -= handler)
                    .Select(evt => evt.EventArgs.FullPath);

            var createdEvents = Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                    handler => _watcher.Created += handler,
                    handler => _watcher.Created -= handler)
                    .Select(evt => evt.EventArgs.FullPath);

            var renamedEvents = Observable.FromEventPattern<RenamedEventHandler, RenamedEventArgs>(
                    handler => _watcher.Renamed += handler,
                    handler => _watcher.Renamed -= handler)
                    .Select(evt => evt.EventArgs.FullPath);

            var folderCreatedEvents = createdEvents.Merge(renamedEvents)
                .Where(Directory.Exists)
                .Select(GetFilesRecursive)
                .SelectMany(f => f);

            var fileCreatedEvents = createdEvents.Merge(renamedEvents)
                .Where(File.Exists);

            var fileChangedEvents = changedEvents
                .Where(File.Exists);

            return Observable
                .Merge(fileCreatedEvents, folderCreatedEvents, fileChangedEvents)
                .Where(_ => _watchSubscription is not null)
                .Publish(paths => paths.Buffer(() => paths.Throttle(TimeSpan.FromMilliseconds(2000))))
                .Select(paths => paths
                    .Distinct()
                    .Select(path => new FileInfo(path))
                    .Where(AcceptedType)
                    .ToList())
                .Subscribe(files => _fileFound.OnNext([.. files]));
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

        private bool AcceptedType(FileInfo fileInfo) => _fileTypes.Any(t => t.Equals(fileInfo.Extension));
        public void Dispose()
        {
            _watchSubscription?.Dispose();
            _watcher?.Dispose();
        }
    }
}