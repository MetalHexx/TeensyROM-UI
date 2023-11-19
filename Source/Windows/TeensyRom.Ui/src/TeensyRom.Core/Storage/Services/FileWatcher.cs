using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace TeensyRom.Core.Storage.Services
{
    /// <summary>
    /// Watches a folder and notifies subscribers that a new
    /// file was added to a folder.
    /// </summary>
    public interface IFileWatcher : IDisposable
    {
        /// <summary>
        /// Emits values when files are added
        /// </summary>
        IObservable<FileInfo> FileFound { get; }

        /// <summary>
        /// Enables the watcher with given a path a file filter
        /// </summary>
        /// <param name="fileTypes">For example *.sid</param>
        void Enable(string fullPath, params string[] fileTypes);

        /// <summary>
        /// Disables the file watcher
        /// </summary>
        void Disable();
    }

    public class FileWatcher : IFileWatcher, IDisposable
    {
        private readonly Subject<FileInfo> _fileFound = new Subject<FileInfo>();
        public IObservable<FileInfo> FileFound => _fileFound.AsObservable();

        private readonly FileSystemWatcher _watcher = new();
        private string[] _fileTypes = Array.Empty<string>();
        private readonly ConcurrentDictionary<string, byte> _processedFiles = new();
        private IDisposable _watchSubscription;

        private bool NotDupe(FileInfo fileInfo) => _processedFiles.TryAdd(fileInfo.Name, 0);
        private bool AcceptedType(FileInfo fileInfo) => _fileTypes.Any(t => t.Equals(fileInfo.Extension));

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
                .Select(filePath => new FileInfo(filePath))
                .Where(AcceptedType)
                .Where(NotDupe)
                .Subscribe(_fileFound);
        }

        public void Dispose()
        {
            _watchSubscription?.Dispose();
            _watcher?.Dispose();
        }
    }
}