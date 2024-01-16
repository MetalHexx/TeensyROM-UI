using DynamicData;
using MediatR;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.State;
using TeensyRom.Ui.Helpers.ViewModel;

namespace TeensyRom.Ui.Features.Connect
{
    public class ConnectViewModel: FeatureViewModelBase, IDisposable
    {
        [Reactive] public bool IsConnected { get; set; }

        [Reactive] public bool IsConnectable { get; set; }
        [Reactive] public string SelectedPort { get; set; } = string.Empty;

        [ObservableAsProperty] public string[]? Ports { get; }
        public ObservableCollection<string> Logs { get; } = [];

        public ReactiveCommand<Unit, Unit> ConnectCommand { get; set; }
        public ReactiveCommand<Unit, Unit> DisconnectCommand { get; set; }
        public ReactiveCommand<Unit, PingResult> PingCommand { get; set; }
        public ReactiveCommand<Unit, ResetResult> ResetCommand { get; set; }
        public ReactiveCommand<Unit, Unit> ClearLogsCommand { get; set; }

        private readonly IDisposable? _portsSubscription;
        private readonly IDisposable? _selectedPortSubscription;
        private readonly IDisposable? _logsSubscription;
        private readonly IMediator _mediator;
        private readonly ILoggingService _logService;
        private readonly StringBuilder _logBuilder = new StringBuilder();
        private readonly int _maxLogEntries = 1000;

        public ConnectViewModel(IMediator mediator, ISerialStateContext serialState, ILoggingService logService)
        {
            FeatureTitle = "Manage Connection";

            _mediator = mediator;
            _logService = logService;
            serialState.Ports.ToPropertyEx(this, vm => vm.Ports);

            serialState.WhenAnyValue(x => x.CurrentState).Subscribe(state =>
            {
                IsConnected = state is SerialConnectedState;
                IsConnectable = state is SerialConnectableState;
            });

            ConnectCommand = ReactiveCommand.Create<Unit, Unit>(n =>
                serialState.OpenPort(), outputScheduler: ImmediateScheduler.Instance);

            DisconnectCommand = ReactiveCommand.Create<Unit, Unit>(n =>
                serialState.ClosePort(), outputScheduler: ImmediateScheduler.Instance);

            PingCommand = ReactiveCommand.CreateFromTask(() => _mediator.Send(new PingCommand()), outputScheduler: RxApp.MainThreadScheduler);

            ResetCommand = ReactiveCommand.CreateFromTask(n =>_mediator.Send(new ResetCommand()), outputScheduler: ImmediateScheduler.Instance);

            ClearLogsCommand = ReactiveCommand.Create<Unit, Unit>(n =>
            {
                Logs.Clear();
                return Unit.Default;
            }, outputScheduler: ImmediateScheduler.Instance);

            _portsSubscription = serialState.Ports
                .Where(ports => ports.Length > 0)
                .Subscribe(ports => SelectedPort = ports.First());

            _selectedPortSubscription = this.WhenAnyValue(x => x.SelectedPort)
                .Where(port => port != null)
                .Subscribe(port => serialState.SetPort(port));

            _logsSubscription = logService.Logs
                .ObserveOn(RxApp.MainThreadScheduler)                
                .Subscribe(logMessage =>
                {
                    Logs.Add(logMessage);
                    if (Logs.Count > _maxLogEntries)
                    {
                        Logs.RemoveAt(0);
                    }
                });
        }

        public void Dispose()
        {
            _portsSubscription?.Dispose();
            _selectedPortSubscription?.Dispose();
            _logsSubscription?.Dispose();
        }
    }
}
