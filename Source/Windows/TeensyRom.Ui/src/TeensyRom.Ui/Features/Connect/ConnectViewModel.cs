using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using TeensyRom.Core.Serial;

namespace TeensyRom.Ui.Features.Connect
{
    public class ConnectViewModel: ReactiveObject
    {
        [ObservableAsProperty]
        public string[]? Ports { get; }

        [Reactive]
        public string SelectedPort { get; set; }

        [Reactive]
        public string Logs { get; set; } = string.Empty;

        public ReactiveCommand<Unit, Unit> ConnectCommand { get; set; }

        public ReactiveCommand<Unit, Unit> PingCommand { get; set; }

        private readonly IObservableSerialPort _serialService;
        private readonly StringBuilder _logBuilder = new StringBuilder();

        public ConnectViewModel(IObservableSerialPort serialService)
        {
            _serialService = serialService;

            _serialService.Ports.ToPropertyEx(this, vm => vm.Ports);

            ConnectCommand = ReactiveCommand.Create<Unit, Unit>(n =>
                _serialService.OpenPort(), outputScheduler: ImmediateScheduler.Instance);

            PingCommand = ReactiveCommand.Create<Unit, Unit>(n =>
                _serialService.PingDevice(), outputScheduler: ImmediateScheduler.Instance);

            this.WhenAnyValue(x => x.SelectedPort)
                .Where(port => port != null)
                .Subscribe(port => _serialService.SetPort(port));

            _serialService.Logs.Subscribe(log => 
            {
                _logBuilder.AppendLine(log);
                Logs = _logBuilder.ToString();
            });

            SelectedPort = Ports![0];
        }
    }
}
