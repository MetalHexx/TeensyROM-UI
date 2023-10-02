using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace TeensyRom.Core.File
{
    public class FileWatcher : IFileWatcher, IDisposable
    {
        private readonly Subject<string> _fileFound = new Subject<string>();
        public IObservable<string> FileFound => _fileFound.AsObservable();

        private readonly FileSystemWatcher _watcher = new();
        private readonly ConcurrentDictionary<string, byte> _processedFiles = new ();

        public void SetWatchPath(string path, string fileFilter)
        {
            _watcher.Path = path;
            _watcher.NotifyFilter = NotifyFilters.LastWrite;
            _watcher.Filter = fileFilter;
            _watcher.Changed -= OnFileChanged;
            _watcher.Changed += OnFileChanged;
            _watcher.EnableRaisingEvents = true;
        }

        private void OnFileChanged(object source, FileSystemEventArgs e) 
        {
            if (NotDupe(e.FullPath))
            {
                _fileFound.OnNext(e.FullPath);
            }
            _fileFound.OnNext(e.FullPath);
        }

        private bool NotDupe(string filePath) => _processedFiles.TryAdd(filePath, 0);

        public void Dispose()
        {
            _watcher.Changed -= OnFileChanged;
            _watcher.Dispose();
        }
    }
}
