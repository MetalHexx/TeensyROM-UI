using Newtonsoft.Json;
using System.Windows.Threading;
using TeensyRom.Core.Commands.File.LaunchFile;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Settings.Entities;
using TeensyRom.Core.Settings.Services;
using TeensyRom.Core.Storage.Services;
using TeensyRom.Core.Storage;
using TeensyRom.Ui.Features.FileTransfer;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Features.Settings;
using TeensyRom.Ui.Features.Connect;

namespace TeensyRom.Tests.Integration
{
    public class TeensyFixture: IDisposable
    {
        public TeensySettings Settings { get; set; } = new TeensySettings();
        public ISettingsService SettingsService { get; private set; }
        public ILoggingService LogService { get; private set; }
        public IObservableSerialPort SerialPort { get; private set; }
        public ISerialPortState SerialState { get; private set; }
        public IFileWatcher FileWatcher { get; private set; }
        public IFileWatchService FileWatchService { get; private set; }        
        public GetDirectoryContentCommand GetDirectoryContentCommand { get; private set; }
        public SaveFileCommand SaveFileCommand { get; private set; }
        public ResetCommand ResetCommand { get; private set; }
        public LaunchFileCommand LaunchFileCommand { get; private set; }
        public PingCommand PingCommand { get; private set; }
        public SettingsViewModel SettingsViewModel { get; private set; }
        public ConnectViewModel ConnectViewModel { get; private set; }
        public FileTransferViewModel FileTransferViewModel { get; private set; }
        public readonly string SettingsFileName = "Settings.json";
        public readonly string TestFileName = $"{Guid.NewGuid().ToString().Substring(0, 7)}-test";
        public string FullSourceTestPath => @$"{Settings.WatchDirectoryLocation}\{TestFileName}";

        public void Initialize(bool initOpenPort = true)
        {
            Settings.InitializeDefaults();
            var json = JsonConvert.SerializeObject(Settings);
            File.WriteAllText(SettingsFileName, json);
            LogService = new LoggingService();
            SerialPort = new ObservableSerialPort(LogService);
            SerialState = new SerialPortState(SerialPort);
            SettingsService = new SettingsService(LogService);
            var snackbar = new SnackbarService(Dispatcher.CurrentDispatcher);
            ResetCommand = new ResetCommand(SettingsService, SerialPort, LogService);
            LaunchFileCommand = new LaunchFileCommand(SettingsService, SerialPort, LogService, ResetCommand);
            PingCommand = new PingCommand(SettingsService, SerialPort, LogService);
            GetDirectoryContentCommand = new GetDirectoryContentCommand(SettingsService, SerialPort, LogService);
            SaveFileCommand = new SaveFileCommand(SettingsService, SerialPort, LogService);
            FileWatcher = new FileWatcher();
            FileWatchService = new FileWatchService(SettingsService, FileWatcher, SerialState, LogService, SaveFileCommand);
            FileTransferViewModel = new FileTransferViewModel(GetDirectoryContentCommand, SettingsService, SerialState, LaunchFileCommand, new NavigationService(), LogService, snackbar, Dispatcher.CurrentDispatcher);
            ConnectViewModel = new ConnectViewModel(PingCommand, SerialPort, LogService, ResetCommand);
            SettingsViewModel = new SettingsViewModel(SettingsService, snackbar, LogService);

            if (initOpenPort)
            {
                SerialPort.SetPort(SerialPort.GetPortNames().First());
                SerialPort.OpenPort();
            }
        }

        public void Dispose()
        {
            SerialPort?.Dispose();
            SaveFileCommand?.Dispose();
            ResetCommand?.Dispose();
            LaunchFileCommand?.Dispose();
            FileWatcher?.Dispose();
        }
    }
}
