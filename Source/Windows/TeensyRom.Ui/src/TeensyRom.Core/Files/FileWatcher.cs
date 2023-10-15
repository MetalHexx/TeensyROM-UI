using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using TeensyRom.Core.Files.Abstractions;

namespace TeensyRom.Core.Files
{
    public class FileWatcher : IFileWatcher, IDisposable
    {
        private readonly Subject<FileInfo> _fileFound = new Subject<FileInfo>();
        public IObservable<FileInfo> FileFound => _fileFound.AsObservable();

        private readonly FileSystemWatcher _watcher = new();
        private string[] _fileTypes = Array.Empty<string>();
        private readonly ConcurrentDictionary<string, byte> _processedFiles = new();
        private readonly IDisposable _watchSubscription;

        public FileWatcher()
        {
            _watchSubscription = InitializeWatcher();
        }

        public IDisposable InitializeWatcher()
        {
            return Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                handler => _watcher.Changed += handler,
                handler => _watcher.Changed -= handler)
                .Select(evt => evt.EventArgs.FullPath)
                .Select(filePath => new FileInfo(filePath))
                .Where(AcceptedType)
                .Where(NotDupe)                
                .Subscribe(_fileFound);
        }
       
        private bool NotDupe(FileInfo fileInfo) => _processedFiles.TryAdd(fileInfo.Name, 0);
        private bool AcceptedType(FileInfo fileInfo) => _fileTypes.Any(t => t.Equals(fileInfo.Extension));

        public void SetWatchParameters(string path, params string[] fileTypes)
        {
            _fileTypes = fileTypes;

            try
            {
                _watcher.Path = path;
                _watcher.NotifyFilter = NotifyFilters.LastWrite;
                _watcher.Filter = "*.*";
                _watcher.EnableRaisingEvents = true;
            }
            catch
            {
                //Just swallow it.  We'll let the user know in the logs.
            }
        }

        public void Dispose()
        {
            _watchSubscription.Dispose();
            _watcher.Dispose();
        }
    }
}