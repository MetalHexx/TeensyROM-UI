using MediatR;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Ui.Helpers.ViewModel;

namespace TeensyRom.Ui.Features.Connect
{
    public class ConnectViewModel: FeatureViewModelBase, IDisposable
    {
        [ObservableAsProperty]
        public string[]? Ports { get; }

        [ObservableAsProperty]
        public bool IsConnected { get; }

        [ObservableAsProperty]
        public bool IsConnectable { get; }

        [Reactive]
        public string SelectedPort { get; set; } = string.Empty;

        [Reactive]
        public string Logs { get; set; } = string.Empty;

        public ReactiveCommand<Unit, Unit> ConnectCommand { get; set; }
        public ReactiveCommand<Unit, Unit> DisconnectCommand { get; set; }
        public ReactiveCommand<Unit, Unit> PingCommand { get; set; }
        public ReactiveCommand<Unit, Unit> ResetCommand { get; set; }
        public ReactiveCommand<Unit, Unit> ClearLogsCommand { get; set; }

        private readonly IDisposable? _portsSubscription;
        private readonly IDisposable? _selectedPortSubscription;
        private readonly IDisposable? _logsSubscription;
        private readonly IMediator _mediator;
        private readonly IObservableSerialPort _serialPort;
        private readonly ILoggingService _logService;
        private readonly StringBuilder _logBuilder = new StringBuilder();

        public ConnectViewModel(IMediator mediator, IObservableSerialPort serialPort, ILoggingService logService)
        {
            FeatureTitle = "Manage Connection";

            _mediator = mediator;
            _serialPort = serialPort;
            _logService = logService;
            _serialPort.Ports.ToPropertyEx(this, vm => vm.Ports);
            _serialPort.IsConnected.ToPropertyEx(this, vm => vm.IsConnected);

            _serialPort.IsRetryingConnection
                .Select(isRetrying => !isRetrying)
                .ToPropertyEx(this, vm => vm.IsConnectable);

            ConnectCommand = ReactiveCommand.Create<System.Reactive.Unit, Unit>(n =>
                _serialPort.OpenPort(), outputScheduler: ImmediateScheduler.Instance);

            DisconnectCommand = ReactiveCommand.Create<Unit, Unit>(n =>
                _serialPort.ClosePort(), outputScheduler: ImmediateScheduler.Instance);

            PingCommand = ReactiveCommand.CreateFromTask(() => _mediator.Send(new PingCommand()), outputScheduler: RxApp.MainThreadScheduler);

            ResetCommand = ReactiveCommand.CreateFromTask(n =>_mediator.Send(new ResetCommand()), outputScheduler: ImmediateScheduler.Instance);

            ClearLogsCommand = ReactiveCommand.Create<Unit, Unit>(n =>
            {
                Logs = string.Empty;
                _logBuilder.Clear();
                return Unit.Default;
            }, outputScheduler: ImmediateScheduler.Instance);

            _portsSubscription = _serialPort.Ports
                .Where(ports => ports.Length > 0)
                .Subscribe(ports => SelectedPort = ports.First());

            _selectedPortSubscription = this.WhenAnyValue(x => x.SelectedPort)
                .Where(port => port != null)
                .Subscribe(port => _serialPort.SetPort(port));

            _logsSubscription = _logService.Logs
                .Select(log => _logBuilder.AppendLine(log))
                .Select(_ => _logBuilder.ToString())
                .Subscribe(logs => 
                {
                    Logs = logs;
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
