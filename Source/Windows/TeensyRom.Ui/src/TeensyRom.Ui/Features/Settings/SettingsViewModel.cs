using MaterialDesignThemes.Wpf;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Settings.Entities;
using TeensyRom.Core.Settings.Services;

namespace TeensyRom.Ui.Features.Settings
{
    public class SettingsViewModel: ReactiveObject, IDisposable
    {
        [Reactive]
        public string Logs { get; set; } = string.Empty;
        public SnackbarMessageQueue SettingsMessageQueue { get; private set; }

        [ObservableAsProperty]
        public TeensySettings Settings { get; set; }

        public ReactiveCommand<Unit, Unit> SaveSettingsCommand { get; set; }

        private readonly ISettingsService _settingsService;
        private readonly ILoggingService _logService;
        private readonly IDisposable _logsSubscription;
        private readonly StringBuilder _logBuilder = new StringBuilder();

        public SettingsViewModel(ISettingsService settings, Dispatcher dispatcher, ILoggingService logService)
        {
            _logService = logService;
            _settingsService = settings;            
            SettingsMessageQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(3), dispatcher);

            _settingsService.Settings.ToPropertyEx(this, vm => vm.Settings);

            SaveSettingsCommand = ReactiveCommand.Create<Unit, Unit>(n =>
                _settingsService.SaveSettings(Settings), outputScheduler: ImmediateScheduler.Instance);

            _logsSubscription = _logService.Logs
                .Where(log => !string.IsNullOrWhiteSpace(log))
                .Do(log => SettingsMessageQueue.Enqueue(log))
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
