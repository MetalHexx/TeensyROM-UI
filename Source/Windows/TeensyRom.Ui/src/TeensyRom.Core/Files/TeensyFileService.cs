using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using TeensyRom.Core.Files.Abstractions;
using TeensyRom.Core.Serial.Abstractions;
using TeensyRom.Core.Settings;

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

        private readonly ISettingsService _settingsService;
        private readonly IFileWatcher _fileWatcher;
        private readonly ITeensyObservableSerialPort _teensyPort;        
        private IDisposable _settingsSubscription;
        private IDisposable _fileWatchSubscription;
        private IDisposable _teensyPortLogSubscription;
        private TeensySettings _settings = new();

        public TeensyFileService(ISettingsService settingsService, IFileWatcher fileWatcher, ITeensyObservableSerialPort serialPort)
        {
            _settingsService = settingsService;
            _fileWatcher = fileWatcher;
            _teensyPort = serialPort;            
            _teensyPortLogSubscription = _teensyPort.Logs.Subscribe(_logs.OnNext, _logs.OnError);
            InitializeSettings();
            InitializeFileWatch();
        }

        private void InitializeSettings()
        {
            _settingsSubscription = _settingsService.Settings
                .Do(settings => _settings = settings)
                .Subscribe(settings => SetWatchFolder(settings));
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

            TransformDestination(fileInfo);

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

        private void TransformDestination(TeensyFileInfo fileInfo)
        {
            fileInfo.StorageType = _settings.TargetType;
            fileInfo.TargetPath = fileInfo.Type switch
            {
                TeensyFileType.Sid => _settings.SidTargetPath,
                TeensyFileType.Prg => _settings.PrgTargetPath,
                TeensyFileType.Crt => _settings.CrtTargetPath,
                TeensyFileType.Hex => _settings.HexTargetPath,
                _ => throw new ArgumentException($"Unsupported file type: {fileInfo.Type}")
            };
        }

        public void SetWatchFolder(TeensySettings settings)
        {
            _fileWatcher.SetWatchParameters(settings.WatchDirectoryLocation, ".sid", ".prg", ".crt");
        }

        public void Dispose()
        {
            _settingsSubscription?.Dispose();
            _fileWatchSubscription?.Dispose();
            _teensyPortLogSubscription?.Dispose();
            _fileWatcher.Dispose();
        }
    }
}