using MediatR;
using System.Collections.Concurrent;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Core.Storage.Services;

namespace TeensyRom.Core.Storage
{
    public interface IFileWatchService : IDisposable 
    {
        IObservable<bool> IsProcessing { get; }
        IObservable<List<TeensyFileInfo>> WatchFiles { get; }
        void ToggleFileWatch(TeensySettings settings);
    }

    /// <summary>
    /// Provides file operation capability.  Leverages the file watcher
    /// to automatically save new files to TeensyROM
    /// </summary>
    public class FileWatchService: IFileWatchService
    {
        public IObservable<List<TeensyFileInfo>> WatchFiles => _watchFiles.AsObservable();
        private readonly Subject<List<TeensyFileInfo>> _watchFiles = new();

        public IObservable<bool> IsProcessing => _isProcessing.AsObservable();
        private readonly BehaviorSubject<bool> _isProcessing = new(false);
        

        private readonly ISettingsService _settingsService;
        private readonly IFileWatcher _fileWatcher;
        private readonly ISerialStateContext _serialState;
        private readonly ICachedStorageService _storageService;
        private IDisposable? _settingsSubscription = null!;
        private IDisposable? _fileWatchSubscription = null!;

        public FileWatchService(ISettingsService settingsService, IFileWatcher fileWatcher, ISerialStateContext serialState, ICachedStorageService storageService)
        {
            _settingsService = settingsService;
            _fileWatcher = fileWatcher;
            _serialState = serialState;
            _storageService = storageService;
            _settingsSubscription = _settingsService.Settings.Subscribe(ToggleFileWatch);
        }

        public void ToggleFileWatch(TeensySettings settings)
        {   
            if(settings.AutoFileCopyEnabled is false)
            {
                _fileWatcher.Disable();
                return;
            }            
            _fileWatcher.Enable(
                settings.WatchDirectoryLocation,
                settings.FileTargets.Select(t => t.Extension).ToArray());

            _fileWatchSubscription ??= _fileWatcher.FilesFound
                .CombineLatest(_serialState.CurrentState, (file, serialState) => (file, serialState))
                .Where(fileSerial => fileSerial.serialState is SerialConnectedState && settings.AutoFileCopyEnabled)
                .Select(fileSerial => fileSerial.file)
                .DistinctUntilChanged()
                .Select(files => files.Select(f => new TeensyFileInfo(f)).ToList())
                .Subscribe(_watchFiles.OnNext);
        }       
               
        public void Dispose()
        {
            _settingsSubscription?.Dispose();
            _fileWatchSubscription?.Dispose();
            _fileWatcher?.Dispose();
        }
    }
}