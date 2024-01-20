using Newtonsoft.Json;
using System.Windows.Threading;
using TeensyRom.Core.Commands.File.LaunchFile;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Storage.Services;
using TeensyRom.Core.Storage;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Features.Settings;
using TeensyRom.Ui.Features.Connect;
using TeensyRom.Core.Settings;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TeensyRom.Core;
using TeensyRom.Ui;
using System.Text;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Tests.Integration
{
    public class TeensyFixture: IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        public IMediator Mediator;

        public const string IntegrationTestPath = "/integration-tests";
        public TeensySettings Settings { get; set; } = new TeensySettings();
        public ISettingsService SettingsService { get; private set; }
        public ILoggingService LogService { get; private set; }
        public IObservableSerialPort SerialPort { get; private set; }
        public IFileWatcher FileWatcher { get; private set; }
        public IFileWatchService FileWatchService { get; private set; }        
        public SettingsViewModel SettingsViewModel { get; private set; }
        public ConnectViewModel ConnectViewModel { get; private set; }
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
            SettingsService = _serviceProvider.GetRequiredService<ISettingsService>();
            FileWatcher = _serviceProvider.GetRequiredService<IFileWatcher>();
            FileWatchService = _serviceProvider.GetRequiredService<IFileWatchService>();
            ConnectViewModel = _serviceProvider.GetRequiredService<ConnectViewModel>();
            SettingsViewModel = _serviceProvider.GetRequiredService<SettingsViewModel>();
            
            if (initOpenPort)
            {
                SerialPort.SetPort(System.IO.Ports.SerialPort.GetPortNames().First());
                SerialPort.OpenPort();
            }
        }

        public TeensyFileInfo CreateTeensyFileInfo(
            TeensyFileType fileType = TeensyFileType.Sid,
            string? targetPath = null,
            TeensyStorageType storageType = TeensyStorageType.SD)
        {
            string extension = GetFileExtension(fileType);
            string tempFileName = $"{Guid.NewGuid().ToString().Substring(0, 7)}{extension}";
            string tempFilePath = Path.Combine(Path.GetTempPath(), tempFileName);

            // Generate default content
            string defaultContent = "Default content for testing.";
            File.WriteAllText(tempFilePath, defaultContent);

            var fileInfo = new TeensyFileInfo(tempFilePath)
            {
                TargetPath = targetPath ?? string.Empty,
                StorageType = storageType
            };

            File.Delete(fileInfo.FullPath);

            return fileInfo;
        }

        private string GetFileExtension(TeensyFileType fileType)
        {
            return fileType switch
            {
                TeensyFileType.Sid => ".sid",
                TeensyFileType.Crt => ".crt",
                TeensyFileType.Prg => ".prg",
                TeensyFileType.Hex => ".hex",
                _ => ".tmp", // default extension for Unknown or other types
            };
        }


        public void Dispose()
        {
            SerialPort?.Dispose();
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
