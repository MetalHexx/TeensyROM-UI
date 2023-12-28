using Newtonsoft.Json;
using System.Windows.Threading;
using TeensyRom.Core.Commands.File.LaunchFile;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Storage.Services;
using TeensyRom.Core.Storage;
using TeensyRom.Ui.Features.FileTransfer;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Features.Settings;
using TeensyRom.Ui.Features.Connect;
using TeensyRom.Core.Settings;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TeensyRom.Core;
using TeensyRom.Ui;

namespace TeensyRom.Tests.Integration
{
    public class TeensyFixture: IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        public IMediator Mediator;

        public TeensySettings Settings { get; set; } = new TeensySettings();
        public ISettingsService SettingsService { get; private set; }
        public ILoggingService LogService { get; private set; }
        public IObservableSerialPort SerialPort { get; private set; }
        public ISerialPortState SerialState { get; private set; }
        public IFileWatcher FileWatcher { get; private set; }
        public IFileWatchService FileWatchService { get; private set; }        
        public IGetDirectoryCommand GetDirectoryContentCommand { get; private set; }
        public ISaveFileCommand SaveFileCommand { get; private set; }
        public IResetCommand ResetCommand { get; private set; }
        public ILaunchFileCommand LaunchFileCommand { get; private set; }
        public SettingsViewModel SettingsViewModel { get; private set; }
        public ConnectViewModel ConnectViewModel { get; private set; }
        public FileTransferViewModel FileTransferViewModel { get; private set; }
        public readonly string SettingsFileName = "Settings.json";
        public readonly string TestFileName = $"{Guid.NewGuid().ToString().Substring(0, 7)}-test";
        public string FullSourceTestPath => @$"{Settings.WatchDirectoryLocation}\{TestFileName}";

        public TeensyFixture()
        {
            var services = new ServiceCollection();
            services.ConfigureTeensyServices(Dispatcher.CurrentDispatcher);
            _serviceProvider = services.BuildServiceProvider();
        }

        public void Initialize(bool initOpenPort = true)
        {
            Settings.InitializeDefaults();
            var json = JsonConvert.SerializeObject(Settings);
            File.WriteAllText(SettingsFileName, json);
            Mediator = _serviceProvider.GetRequiredService<IMediator>();
            LogService = _serviceProvider.GetRequiredService<ILoggingService>();
            SerialPort = _serviceProvider.GetRequiredService<IObservableSerialPort>();
            SerialState = _serviceProvider.GetRequiredService<ISerialPortState>();
            SettingsService = _serviceProvider.GetRequiredService<ISettingsService>();
            FileWatcher = _serviceProvider.GetRequiredService<IFileWatcher>();
            FileWatchService = _serviceProvider.GetRequiredService<IFileWatchService>();
            FileTransferViewModel = _serviceProvider.GetRequiredService<FileTransferViewModel>();
            ConnectViewModel = _serviceProvider.GetRequiredService<ConnectViewModel>();
            SettingsViewModel = _serviceProvider.GetRequiredService<SettingsViewModel>();
            GetDirectoryContentCommand = _serviceProvider.GetRequiredService<IGetDirectoryCommand>();
            SaveFileCommand = _serviceProvider.GetRequiredService<ISaveFileCommand>();
            ResetCommand = _serviceProvider.GetRequiredService<IResetCommand>();
            LaunchFileCommand = _serviceProvider.GetRequiredService<ILaunchFileCommand>();
            
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

            if (File.Exists(FullSourceTestPath))
            {
                File.Delete(FullSourceTestPath);
            }

            if (File.Exists(SettingsFileName))
            {
                File.Delete(SettingsFileName);
            }
        }
    }
}
