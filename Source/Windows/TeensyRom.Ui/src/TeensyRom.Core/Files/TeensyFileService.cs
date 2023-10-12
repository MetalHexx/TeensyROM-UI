using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using TeensyRom.Core.Files.Abstractions;
using TeensyRom.Core.Serial.Abstractions;

namespace TeensyRom.Core.Files
{
    /// <summary>
    /// Provides file operation capability.  Leverages the file watcher
    /// to automatically save new files to TeensyROM
    /// </summary>
    public class TeensyFileService: ITeensyFileService
    {
        protected readonly Subject<string> _logs = new();
        public IObservable<string> Logs => _logs.AsObservable();

        private readonly IFileWatcher _fileWatcher;
        private readonly ITeensyObservableSerialPort _teensyPort;        
        private IDisposable _fileWatchSubscription;
        private IDisposable _teensyPortLogSubscription;

        public TeensyFileService(IFileWatcher fileWatcher, ITeensyObservableSerialPort serialPort)
        {
            _fileWatcher = fileWatcher;
            _teensyPort = serialPort;
            _fileWatcher.SetWatchPath(GetDefaultBrowserDownloadPath(), "*.sid"); //TODO: Make this configurable in UI
            _teensyPortLogSubscription = _teensyPort.Logs.Subscribe(_logs.OnNext, _logs.OnError);
            InitializeFileWatch();
        }

        private void InitializeFileWatch()
        {
            _fileWatchSubscription = _fileWatcher.FileFound
                .Throttle(TimeSpan.FromMilliseconds(500))
                .WithLatestFrom(_teensyPort.IsConnected, (f, c) => new { File = f, IsConnected = c })
                .Where(fc => fc.IsConnected)
                .Select(fc => new TeensyFileInfo(fc.File))
                .Do(fileInfo => _logs.OnNext($"File detected: {fileInfo.FullPath}"))
                .Subscribe(fileInfo => SaveFile(fileInfo));
        }

        /// <summary>
        /// Grabs the default operating system from the users profile. 
        /// </summary>
        /// <returns></returns>
        private string GetDefaultBrowserDownloadPath()
        {
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(userProfile, "Downloads");  //TODO: make this user configurable
        }

        public Unit SaveFile(string path)
        {
            TeensyFileInfo fileInfo;

            try
            {                
                fileInfo = new TeensyFileInfo(path);
                SaveFile(fileInfo);
            }
            catch (FileNotFoundException ex)
            {
                _logs.OnNext(ex.Message);
            }
            return Unit.Default;
        }

        public Unit SaveFile(TeensyFileInfo fileInfo)
        {
            _logs.OnNext("Initiating file transfer handshake");

            if (_teensyPort.SendFile(fileInfo))
            {
                _logs.OnNext($"Saved: {fileInfo}");
            }
            else
            {
                _logs.OnNext($"Failed to save: {fileInfo}");
            }
            return Unit.Default;
        }

        public void SetWatchFolder(string fullPath)
        {
            //TODO: Implement user watch folder preference
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _fileWatchSubscription?.Dispose();
            _teensyPortLogSubscription?.Dispose();
            _fileWatcher.Dispose();
        }
    }
}