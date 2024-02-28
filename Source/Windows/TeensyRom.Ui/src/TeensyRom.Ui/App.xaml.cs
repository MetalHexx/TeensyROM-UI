using CommonServiceLocator;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Main;
using System.Reflection;
using TeensyRom.Core.Storage;
using TeensyRom.Core.Settings;
using System.Reactive.Threading.Tasks;
using TeensyRom.Core.Serial.State;
using System.Diagnostics;
using System.IO;
using System.Windows.Threading;
using TeensyRom.Core.Logging;
using System.Collections.Generic;
using TeensyRom.Core.Games;
using TeensyRom.Core.Music.Sid;
using TeensyRom.Core.Common;
using System.Threading.Tasks;

namespace TeensyRom.Ui
{
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;

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
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            var mainViewModel = _serviceProvider.GetRequiredService<NavigationHostViewModel>();
            mainWindow.DataContext = mainViewModel;
            mainWindow.Show();
            _ = _serviceProvider.GetRequiredService<IFileWatchService>(); //triggers file watch service to construct and start

            _ = Task.Run(UnpackAssets);
        }

        private Task UnpackAssets()
        {
            AssetHelper.UnpackImages(GameConstants.Game_Image_Local_Path, "OneLoad64.zip");
            AssetHelper.UnpackImages(SidConstants.Musician_Image_Local_Path, "Composers.zip");
            return Task.CompletedTask;
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
                alertService.Publish("There was an unhandled exception.  Please check TeensyErrorLogs.txt for more information.");
            }
        }

        private static void LogExceptionToFile(Exception ex)
        {
            string filePath = "TeensyErrorLogs.txt";

            try
            {
                lock (filePath)
                {
                    File.AppendAllText(filePath, $"{DateTime.Now}{Environment.NewLine}Exception: {ex}{Environment.NewLine}{Environment.NewLine}");
                }
            }
            catch (Exception logEx)
            {
                Debug.WriteLine("Failed to log exception: " + logEx.Message);
            }
        }
    }
}