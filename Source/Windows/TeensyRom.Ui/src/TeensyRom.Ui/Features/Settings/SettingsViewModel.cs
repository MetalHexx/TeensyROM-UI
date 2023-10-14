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
using TeensyRom.Core.Settings;

namespace TeensyRom.Ui.Features.Settings
{
    public class SettingsViewModel: ReactiveObject, IDisposable
    {
        [Reactive]
        public string Logs { get; set; } = string.Empty;

        [ObservableAsProperty]
        public TeensySettings Settings { get; set; }

        public ReactiveCommand<Unit, Unit> SaveSettingsCommand { get; set; }

        private readonly ISettingsService _settingsService;
        private readonly IDisposable _logsSubscription;
        private readonly StringBuilder _logBuilder = new StringBuilder();

        public SettingsViewModel(ISettingsService settings)
        {
            _settingsService = settings;

            _settingsService.Settings.ToPropertyEx(this, vm => vm.Settings);

            SaveSettingsCommand = ReactiveCommand.Create<Unit, Unit>(n =>
                _settingsService.SaveSettings(Settings), outputScheduler: ImmediateScheduler.Instance);

            _logsSubscription = _settingsService.Logs
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
