using MediatR;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using TeensyRom.Ui.Core.Commands;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial.State;
using TeensyRom.Ui.Controls.FeatureTitle;
using TeensyRom.Ui.Features.Terminal.SerialCommand;
using TeensyRom.Ui.Services;
using TeensyRom.Core.Settings;

namespace TeensyRom.Ui.Features.Terminal
{
    public class TerminalViewModel : ReactiveObject
    {
        [ObservableAsProperty] public List<string> Ports { get; }
        [ObservableAsProperty] public bool IsConnected { get; set; }
        [ObservableAsProperty] public bool IsConnectable { get; set; }

        [Reactive] public FeatureTitleViewModel Title { get; set; }
        [Reactive] public string SelectedPort { get; set; } = string.Empty;

        public ReactiveCommand<Unit, Unit> ConnectCommand { get; set; }
        public ReactiveCommand<Unit, Unit> DisconnectCommand { get; set; }
        public ReactiveCommand<Unit, PingResult> PingCommand { get; set; }
        public ReactiveCommand<Unit, ResetResult> ResetCommand { get; set; }
        public ReactiveCommand<Unit, Unit> ClearLogsCommand { get; set; }
        public LogViewModel Log { get; } = new();
        public ISerialCommandViewModel SerialCommandVm { get; }

        private readonly IMediator _mediator;
        private readonly ISerialStateContext _serial;
        private readonly ILoggingService _log;
        private readonly IAlertService _alert;
        private readonly ISettingsService _settingsService;
        private bool _nfcWarningFlag = false;

        public TerminalViewModel(IMediator mediator, ISerialStateContext serial, ILoggingService log, IAlertService alertService, IDialogService dialog, ISerialCommandViewModel serialCommandVm, ISettingsService settingsService)
        {
            Title = new FeatureTitleViewModel("Terminal");
            SerialCommandVm = serialCommandVm;
            _settingsService = settingsService;
            serial.Ports
                .Where(p => p is not null && p.Length > 0)
                .Select(p => p.ToList())
                .Select(p =>
                {
                    if (p.Count > 0)
                    {
                        p.Insert(0, "Auto-detect");
                    }
                    return p;
                })
                .ToPropertyEx(this, vm => vm.Ports);

            this.WhenAnyValue(x => x.Ports)
                .Where(port => port != null)
                .Subscribe(ports => SelectedPort = ports.First());

            this.WhenAnyValue(x => x.SelectedPort)
                .Where(port => port is not null && !port!.Contains("Auto-detect"))
                .Subscribe(port => serial.SetPort(port));

            serial.CurrentState
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(state => state is SerialConnectedState or SerialBusyState)
                .ToPropertyEx(this, vm => vm.IsConnected);

            serial.CurrentState
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(state => state is SerialConnectableState)
                .ToPropertyEx(this, vm => vm.IsConnectable);

            ConnectCommand = ReactiveCommand.CreateFromTask<Unit, Unit>(
                execute: async n =>
                {
                    var success = await TrySingleConnect();

                    if (!success)
                    {
                        alertService.Publish("Failed to connect to device.  Check to make sure you're using the correct com port.");
                    }

                    return Unit.Default;
                },
                canExecute: this.WhenAnyValue(x => x.IsConnectable),
                outputScheduler: RxApp.MainThreadScheduler);

            DisconnectCommand = ReactiveCommand.Create<Unit, Unit>(
                execute: n => serial.ClosePort(),
                canExecute: this.WhenAnyValue(x => x.IsConnected),
                outputScheduler: RxApp.MainThreadScheduler);

            PingCommand = ReactiveCommand.CreateFromTask(
                execute: () => mediator.Send(new PingCommand()),
                canExecute: this.WhenAnyValue(x => x.IsConnected),
                outputScheduler: RxApp.MainThreadScheduler);

            ResetCommand = ReactiveCommand.CreateFromTask(
                execute: () => mediator.Send(new ResetCommand()),
                canExecute: this.WhenAnyValue(x => x.IsConnected),
                outputScheduler: RxApp.MainThreadScheduler);

            ClearLogsCommand = ReactiveCommand.Create<Unit, Unit>(
                execute: _ =>
                {
                    Log.Clear();
                    return Unit.Default;
                },
                canExecute: this.WhenAnyValue(x => x.Log.Logs.Count).Select(count => count > 0),
                outputScheduler: RxApp.MainThreadScheduler);

            log.Logs
                .Buffer(TimeSpan.FromMilliseconds(1000))
                .Where(logs => logs.Any())
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(logs =>
                {
                    var combinedLog = string.Join("\n", logs);
                    combinedLog.TrimStart('\n');
                    if (combinedLog.Contains("PN53x board not found") && _nfcWarningFlag == false)
                    {
                        _nfcWarningFlag = true;
                        dialog.ShowConfirmation("NFC Not Found", "The TR is having an issue locating your NFC device.  This will cause a degraded experience with the Desktop UI.\r\rPlease plug in your NFC device or disable it by pressing F8 and 'F' on the C64.");
                    }
                    Log.AddLog(combinedLog);
                });
            _mediator = mediator;
            _serial = serial;
            _log = log;
            _alert = alertService;
            var settings = _settingsService.GetSettings();

            _serial.CurrentState
                 .Where(_ => settings.AutoConnectEnabled && !settings.FirstTimeSetup)
                 .OfType<SerialConnectableState>()
                 .Delay(TimeSpan.FromSeconds(1))
                 .Take(1)
                 .Subscribe(async _ => await TrySingleConnect());
        }

        private async Task<bool> TrySingleConnect()
        {
            try
            {
                _alert.Publish("Attempting to connect to TeensyROM cartridge.");
                _serial.OpenPort();
            }
            catch(Exception ex)
            {
                _log.InternalError($"Failed to find a connectable TeensyROM cartridge.");
                return false;
            }
            return true;
        }
    }
}
