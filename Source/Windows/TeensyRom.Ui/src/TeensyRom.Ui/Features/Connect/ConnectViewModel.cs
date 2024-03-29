using DynamicData;
using DynamicData.Kernel;
using MediatR;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.State;
using TeensyRom.Ui.Controls.FeatureTitle;
using TeensyRom.Ui.Helpers.ViewModel;

namespace TeensyRom.Ui.Features.Connect
{
    public class ConnectViewModel: ReactiveObject
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

        private readonly IMediator _mediator;
        private readonly ISerialStateContext _serial;
        private readonly ILoggingService _log;

        public ConnectViewModel(IMediator mediator, ISerialStateContext serial, ILoggingService log, IAlertService alertService)
        {
            Title = new FeatureTitleViewModel("Connection");

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
                    var success = SelectedPort.Contains("Auto-detect") 
                        ? await TryAutoConnect()
                        : await TrySingleConnect();

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

            //This works
            //log.Logs
            //    .ObserveOn(RxApp.MainThreadScheduler)
            //    .Subscribe(Log.AddLog);

            //This one does not
            log.Logs
                .Buffer(TimeSpan.FromMilliseconds(1000))
                .Where(logs => logs.Any())
                .ObserveOn(RxApp.MainThreadScheduler) // Ensure this is right before Subscribe
                .Subscribe(logs =>
                {
                    var combinedLog = string.Join("\n", logs); // Join the logs using a newline separator
                    combinedLog.TrimStart('\n');
                    Log.AddLog(combinedLog);
                });



            _mediator = mediator;
            _serial = serial;
            _log = log;
        }

        private async Task<bool> TrySingleConnect()
        {
            _serial.OpenPort();

            var serialState = await _serial.CurrentState.FirstAsync();

            if (serialState is not SerialConnectedState)
            {
                _log.InternalError($"Failed to find a connectable TeensyROM cartridge.");
                return false;
            }
            return true;
        }

        private async Task<bool> TryAutoConnect()
        {
            _log.Internal("Scanning for a TR on each COM port.");

            var legitPorts = Ports!
                .Where(p => p != "Auto-detect")
                .OrderBy(p => p);

            foreach (var port in legitPorts)
            {
                _serial.SetPort(port);
                _serial.OpenPort();
                var serialState = await _serial.CurrentState.FirstAsync();

                if(serialState is not SerialConnectedState)
                {
                    _log.Internal($"Attempting the next COM port.");
                    continue;
                }
                return true;
            }
            _log.Internal($"Failed to find a connectable TeensyROM cartridge.");
            return false;
        }
    }
}
