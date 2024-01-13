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

namespace TeensyRom.Ui
{
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;

        public App()
        {
            ServiceCollection services = new ServiceCollection();
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
        }
    }
}
