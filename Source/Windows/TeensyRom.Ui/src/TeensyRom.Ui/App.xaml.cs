using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Main;
using System.Reflection;
using TeensyRom.Core.Storage;
using System.Diagnostics;
using System.IO;
using System.Windows.Threading;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Games;
using TeensyRom.Core.Common;
using System.Threading.Tasks;
using TeensyRom.Core.Music;
using TeensyRom.Core.Assets;
using TeensyRom.Core.Storage.Tools.D64Extraction;
using TeensyRom.Core.Storage.Tools.Zip;
using TeensyRom.Ui.Services.Process;
using TeensyRom.Core.Midi;

namespace TeensyRom.Ui
{
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;
        private string _logId = FileHelper.GetFileDateTimeStamp(DateTime.Now);

        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            DispatcherUnhandledException += App_DispatcherUnhandledException;

            ServiceCollection services = new();
            services.ConfigureTeensyServices(Current.Dispatcher);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            CreateErrorLogFile();

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            var mainViewModel = _serviceProvider.GetRequiredService<NavigationHostViewModel>();
            mainWindow.DataContext = mainViewModel;
            mainWindow.Show();

            StartBackgroundProcesses();            
            CleanupOldAssets();
            Exit += OnAppExit;
        }

        private void OnAppExit(object sender, ExitEventArgs e)
        {
            var midiService =  _serviceProvider.GetService<IMidiService>();
            midiService?.Dispose();
        }

        private void StartBackgroundProcesses()
        {
            _ = _serviceProvider.GetRequiredService<IFileWatchService>();
            _ = _serviceProvider.GetRequiredService<ICrossProcessService>();
            _ = Task.Run(UnpackAssets);
        }

        private Task UnpackAssets()
        {
            AssetHelper.UnpackAssets(GameConstants.Game_Image_Local_Path, "OneLoad64.zip");
            AssetHelper.UnpackAssets(MusicConstants.Musician_Image_Local_Path, "Composers.zip");
            AssetHelper.UnpackAssets(AssetConstants.VicePath, "vice-bins.zip");
            AssetHelper.UnpackAssets(MusicConstants.DeepSid_Db_Local_Path, "deepsid_db.zip");
            return Task.CompletedTask;
        }

        private void CleanupOldAssets() 
        {
            var d64Service = _serviceProvider.GetRequiredService<ID64Extractor>();
            var zipService = _serviceProvider.GetRequiredService<IZipExtractor>();
            d64Service.ClearOutputDirectory();
            zipService.ClearOutputDirectory();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleException(e.ExceptionObject as Exception);
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            HandleException(e.Exception);
            e.Handled = true;
        }

        private void HandleException(Exception? ex)
        {
          if (ex is not null) 
            {
                LogExceptionToFile(ex);
                var alertService = _serviceProvider.GetRequiredService<IAlertService>();
                alertService.Publish("There was an unhandled exception.");
            }
        }

        private static readonly object _logFileLock = new object();
        private void LogExceptionToFile(Exception ex)
        {
            string filePath = GetUnhandledExceptionLogPath();

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            }

            try
            {
                lock (_logFileLock)
                {
                    File.AppendAllText(filePath, $"{DateTime.Now}{Environment.NewLine}Exception: {ex}{Environment.NewLine}{Environment.NewLine}");
                }
            }
            catch (Exception logEx)
            {
                Debug.WriteLine("Failed to log exception: " + logEx.Message);
            }
        }

        private string GetUnhandledExceptionLogPath() => Path.Combine(Assembly.GetExecutingAssembly().GetPath(), LogConstants.UnhandedErrorLogPath,  $@"{LogConstants.UnhandledLogFileName}{_logId}.{LogConstants.UnhandledLogFileExtention}");

        private void CreateErrorLogFile() 
        {
            string directory = Path.GetDirectoryName(GetUnhandledExceptionLogPath())!;            
            FileHelper.DeleteFilesOlderThan(DateTime.Now.Subtract(TimeSpan.FromDays(7)), directory);

            if (!Directory.Exists(Path.GetDirectoryName(LogConstants.LogPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LogConstants.LogPath)!);
            }
            File.Create(GetUnhandledExceptionLogPath()).Close();
        }
    }
}