using DynamicData;
using DynamicData.Kernel;
using MediatR;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.State;
using TeensyRom.Ui.Features.Global;
using TeensyRom.Ui.Helpers.ViewModel;

namespace TeensyRom.Ui.Features.Connect
{
    public class ConnectViewModel: FeatureViewModelBase
    {
        [ObservableAsProperty] public string[]? Ports { get; }
        [ObservableAsProperty] public bool IsConnected { get; set; }
        [ObservableAsProperty] public bool IsConnectable { get; set; }

        [Reactive] public string SelectedPort { get; set; } = string.Empty;

        public ReactiveCommand<Unit, Unit> ConnectCommand { get; set; }
        public ReactiveCommand<Unit, Unit> DisconnectCommand { get; set; }
        public ReactiveCommand<Unit, PingResult> PingCommand { get; set; }
        public ReactiveCommand<Unit, ResetResult> ResetCommand { get; set; }
        public ReactiveCommand<Unit, Unit> ClearLogsCommand { get; set; }
        public ObservableCollection<string> Logs { get; } = [];

        public ConnectViewModel(IMediator mediator, ISerialStateContext serial, IGlobalState globalState, ILoggingService log)
        {
            FeatureTitle = "Manage Connection";

            serial.Ports.ToPropertyEx(this, vm => vm.Ports);

            serial.Ports
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(ports => ports.Length > 0)
                .Subscribe(ports => SelectedPort = ports.First());

            this.WhenAnyValue(x => x.SelectedPort)
                .Where(port => port != null)
                .Subscribe(port => serial.SetPort(port));

            globalState.SerialConnected
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, vm => vm.IsConnected);

            serial.CurrentState
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(state => state is SerialConnectableState)                
                .ToPropertyEx(this, vm => vm.IsConnectable);

            ConnectCommand = ReactiveCommand.Create<Unit, Unit>(
                execute: n => serial.OpenPort(),
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
                    Logs.Clear();
                    return Unit.Default;
                },
                outputScheduler: RxApp.MainThreadScheduler);

            log.Logs
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(logMessage =>
                {
                    Logs.Add(logMessage);

                    if (Logs.Count > 1000)
                    {
                        Logs.RemoveAt(0);
                    }
                });
        }
    }
}
