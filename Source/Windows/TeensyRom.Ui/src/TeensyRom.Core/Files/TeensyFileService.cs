using Newtonsoft.Json;
using System.Drawing;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json.Nodes;
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
        }

        private void InitializeSettings()
        {
            _settingsSubscription = _settingsService.Settings
                .Do(settings => _settings = settings)
                .Subscribe(settings => ToggleFileWatch(settings));
        }

        private void ToggleFileWatch(TeensySettings settings)
        {
            if(settings.AutoFileCopyEnabled is false)
            {
                _fileWatcher.Disable();
                return;
            }            
            _fileWatcher.Enable(
                settings.WatchDirectoryLocation,
                settings.FileTargets.Select(t => t.Extension).ToArray());

            _fileWatchSubscription ??= _fileWatcher.FileFound
                .Throttle(TimeSpan.FromMilliseconds(500))
                .WithLatestFrom(_teensyPort.IsConnected, (f, c) => new { File = f, IsConnected = c })
                .Where(fc => fc.IsConnected && settings.AutoFileCopyEnabled)
                .Select(fc => new TeensyFileInfo(fc.File))
                .Do(fileInfo => _logs.OnNext($"File detected: {fileInfo.FullPath}"))
                .Subscribe(fileInfo => SaveFile(fileInfo));

            _teensyPort.Logs.Subscribe(_logs.OnNext);
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

            var target = _settings.FileTargets
                .FirstOrDefault(t => t.Type == fileInfo.Type);

            if(target is null)
            {
                throw new ArgumentException($"Unsupported file type: {fileInfo.Type}");
            }

            fileInfo.TargetPath = _settings.TargetRootPath
                .UnixPathCombine(target.TargetPath)
                .EnsureUnixPathEnding();
        }

        public DirectoryContent? GetDirectoryContent(string path, int numItemsToFetch)
        {
            DirectoryContent directoryContent = new();
            uint take = 5;

            for (uint i = 0; i < numItemsToFetch; i += take)
            {
                var page = _teensyPort.GetDirectoryContent(path, _settings.TargetType, i, take);

                if(page is null)
                {
                    _logs.OnNext("There was an error.  Received a null result from the request");
                    return directoryContent;
                }
                directoryContent.Add(page);

                if (page.TotalCount < take)
                {
                    _logs.OnNext($"Reached the end of the directory contents.  Only {directoryContent.TotalCount} / {numItemsToFetch} were fetched.");
                    break;
                }
            }
            _logs.OnNext($"Received the following directory contents:");
            _logs.OnNext(JsonConvert.SerializeObject(directoryContent, new JsonSerializerSettings { Formatting = Formatting.Indented}));
            return directoryContent;
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