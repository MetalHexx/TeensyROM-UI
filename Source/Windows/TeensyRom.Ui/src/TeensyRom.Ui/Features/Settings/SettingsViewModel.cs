using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using TeensyRom.Core.Common;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Services;
using TeensyRom.Ui.Helpers.ViewModel;
using TeensyRom.Ui.Services;

namespace TeensyRom.Ui.Features.Settings
{
    public class SettingsViewModel: FeatureViewModelBase, IDisposable
    {
        [Reactive] public string Logs { get; set; } = string.Empty;
        [ObservableAsProperty] public bool IsDirty { get; }
        [ObservableAsProperty] public TeensySettings? Settings { get; }
        public Interaction<string, bool> ConfirmSave { get; } = new Interaction<string, bool>();

        public ReactiveCommand<Unit, Unit> SaveSettingsCommand { get; set; }

        private readonly ISettingsService _settingsService;
        private readonly ICachedStorageService _storage;
        private readonly IAlertService _alert;
        private readonly IDialogService _dialog;
        private readonly ILoggingService _logService;
        private readonly IDisposable _logsSubscription;
        private readonly StringBuilder _logBuilder = new StringBuilder();

        public SettingsViewModel(ISettingsService settings, ICachedStorageService storage, IAlertService alert, IDialogService dialog, ILoggingService logService)
        {
            FeatureTitle = "Settings";

            _logService = logService;
            _settingsService = settings;
            _storage = storage;
            _alert = alert;
            _dialog = dialog;
            _settingsService.Settings.ToPropertyEx(this, vm => vm.Settings);

            SaveSettingsCommand = ReactiveCommand.CreateFromTask<Unit, Unit>(
                execute: async n => await HandleSave(),
                outputScheduler: RxApp.MainThreadScheduler);

            _logsSubscription = _logService.Logs
                .Where(log => !string.IsNullOrWhiteSpace(log))
                .Select(log => _logBuilder.AppendLineRolling(log))
                .Select(_ => _logBuilder.ToString())
                .Subscribe(logs =>
                {
                    Logs = logs;
                });
        }

        private async Task<Unit> HandleSave()
        {
            var confirmed = await _dialog.ShowConfirmation("Are you sure you want to save settings?\r\rWarning: Cache will be deleted if you proceed.");

            if (!confirmed) return Unit.Default;

            var success = _settingsService.SaveSettings(Settings!);
            if (success)
            {
                _alert.Publish("Settings saved successfully.");
                _storage.ClearCache();
            }
            else
            {
                _alert.Publish("Error saving settings");
            }
            return Unit.Default;
        }

        public void Dispose()
        {
            _logsSubscription?.Dispose();
        }
    }
}