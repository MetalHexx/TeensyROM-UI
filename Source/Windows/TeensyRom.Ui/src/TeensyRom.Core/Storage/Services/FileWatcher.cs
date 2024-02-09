using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace TeensyRom.Core.Storage.Services
{

    public class FileWatcher : IFileWatcher, IDisposable
    {
        private readonly Subject<FileInfo> _fileFound = new Subject<FileInfo>();
        public IObservable<FileInfo> FileFound => _fileFound.AsObservable();

        private readonly FileSystemWatcher _watcher = new();
        private string[] _fileTypes = Array.Empty<string>();
        private readonly ConcurrentDictionary<string, byte> _processedFiles = new();
        private IDisposable? _watchSubscription;

        public void Disable()
        {
            _watchSubscription?.Dispose();
            _watchSubscription = null;
        }

        public void Enable(string path, params string[] fileTypes)
        {
            _fileTypes = fileTypes;

            _watchSubscription ??= InitializeWatcher();

            _watcher.Path = path;
            _watcher.NotifyFilter = NotifyFilters.LastWrite;
            _watcher.Filter = "*.*";
            _watcher.EnableRaisingEvents = true;
        }

        private IDisposable InitializeWatcher()
        {
            return Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                handler => _watcher.Changed += handler,
                handler => _watcher.Changed -= handler)
                .Where(_ => _watchSubscription is not null)
                .Select(evt => evt.EventArgs.FullPath)
                .DistinctUntilChanged()
                .Select(filePath => new FileInfo(filePath))
                .Where(AcceptedType)
                .Where(NotDupe)
                .Subscribe(_fileFound);
        }

        private bool NotDupe(FileInfo fileInfo) => _processedFiles.TryAdd(fileInfo.Name, 0);
        private bool AcceptedType(FileInfo fileInfo) => _fileTypes.Any(t => t.Equals(fileInfo.Extension));
        public void Dispose()
        {
            _watchSubscription?.Dispose();
            _watcher?.Dispose();
        }
    }
}