using System.Collections.Concurrent;
using System.Data;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using TeensyRom.Cli.Core.Common;

namespace TeensyRom.Cli.Core.Storage.Services
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
                Path = path.GetOsFriendlyPath(),
                NotifyFilter = NotifyFilters.LastWrite,
                Filter = "*.*",
                EnableRaisingEvents = true,
                InternalBufferSize = 65536
            };
            _watchSubscription = InitializeWatcher();
        }

        private IDisposable InitializeWatcher()
        {
            if (_watcher is null)
                throw new InvalidOperationException("Watcher not initialized");

            return Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                handler => _watcher.Changed += handler,
                handler => _watcher.Changed -= handler)
                .Where(_ => _watchSubscription is not null)
                .Publish(_events => _events.Buffer(() => _events.Throttle(TimeSpan.FromMilliseconds(2000))))
                .Select(evts => evts
                    .Select(evt => evt.EventArgs.FullPath)
                    .Distinct()
                    .Select(path => new FileInfo(path))
                    .Where(AcceptedType)
                    .ToList())
                .Subscribe(files => _fileFound.OnNext([.. files]));

        }
        private bool AcceptedType(FileInfo fileInfo) => _fileTypes.Any(t => t.Equals(fileInfo.Extension));
        public void Dispose()
        {
            _watchSubscription?.Dispose();
            _watcher?.Dispose();
        }
    }
}