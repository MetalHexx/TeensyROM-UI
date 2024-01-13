using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Windows.Threading;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Settings;
using TeensyRom.Ui.Helpers.ViewModel;
using TeensyRom.Ui.Services;

namespace TeensyRom.Ui.Features.Settings
{
    public class SettingsViewModel: FeatureViewModelBase, IDisposable
    {
        [Reactive]
        public string Logs { get; set; } = string.Empty;

        [ObservableAsProperty]
        public TeensySettings Settings { get; set; }

        public ReactiveCommand<Unit, Unit> SaveSettingsCommand { get; set; }

        private readonly ISettingsService _settingsService;
        private readonly ILoggingService _logService;
        private readonly IDisposable _logsSubscription;
        private readonly StringBuilder _logBuilder = new StringBuilder();

        public SettingsViewModel(ISettingsService settings, IAlertService alert, ILoggingService logService)
        {
            FeatureTitle = "Settings";

            _logService = logService;
            _settingsService = settings;            

            _settingsService.Settings.ToPropertyEx(this, vm => vm.Settings);

            SaveSettingsCommand = ReactiveCommand.Create<Unit, Unit>(n => 
            {
                var success = _settingsService.SaveSettings(Settings);
                if (success)
                {
                    alert.Enqueue("Settings saved successfully.");
                }
                else
                {
                    alert.Enqueue("Error saving settings");
                }
                return Unit.Default;
            }, outputScheduler: ImmediateScheduler.Instance);

            _logsSubscription = _logService.Logs
                .Where(log => !string.IsNullOrWhiteSpace(log))
                .Select(log => _logBuilder.AppendLine(log))
                .Select(_ => _logBuilder.ToString())
                .Subscribe(logs =>
                {
                    Logs = logs;
                });
        }

        public void Dispose()
        {
            _logsSubscription?.Dispose();
        }
    }
}
