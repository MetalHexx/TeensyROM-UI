using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using TeensyRom.Core.Serial;

namespace TeensyRom.Core.File
{
    public class TeensyFileService: ITeensyFileService
    {
        protected readonly Subject<string> _logs = new Subject<string>();
        public IObservable<string> Logs => _logs.AsObservable();

        private readonly IFileWatcher _fileWatcher;
        private readonly ITeensyObservableSerialPort _serialPort;        
        private IDisposable _fileWatchSubscription;

        public TeensyFileService(IFileWatcher fileWatcher, ITeensyObservableSerialPort serialPort)
        {
            _fileWatcher = fileWatcher;
            _serialPort = serialPort;
            
            _fileWatchSubscription = _fileWatcher.FileFound
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Subscribe(filePath => SaveFile(filePath));

            _fileWatcher.SetWatchPath(GetDefaultBrowserDownloadPath(), "*.sid");
        }

        private string GetDefaultBrowserDownloadPath()
        {
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(userProfile, "Downloads");
        }

        public Unit SaveFile(string filePath)
        {
            //TODO: add configuration for user preferenace on file target location
            //TODO: add code to save to the teensyrom via serial port
            _logs.OnNext($"Saved: {filePath}");
            return Unit.Default;
        }

        public void Dispose()
        {
            _fileWatchSubscription?.Dispose();
            _fileWatcher.Dispose();
        }
    }
}
