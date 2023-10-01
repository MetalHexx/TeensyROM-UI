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

        [ObservableAsProperty]
        public bool IsConnected { get; }

        [Reactive]
        public string SelectedPort { get; set; }

        [Reactive]
        public string Logs { get; set; } = string.Empty;

        public ReactiveCommand<Unit, Unit> ConnectCommand { get; set; }

        public ReactiveCommand<Unit, Unit> PingCommand { get; set; }

        private readonly ITeensyObservableSerialPort _teensySerial;
        private readonly StringBuilder _logBuilder = new StringBuilder();

        public ConnectViewModel(ITeensyObservableSerialPort teensySerial)
        {
            _teensySerial = teensySerial;

            _teensySerial.Ports.ToPropertyEx(this, vm => vm.Ports);
            _teensySerial.IsConnected.ToPropertyEx(this, vm => vm.IsConnected);

            ConnectCommand = ReactiveCommand.Create<Unit, Unit>(n =>
                _teensySerial.OpenPort(), outputScheduler: ImmediateScheduler.Instance);

            PingCommand = ReactiveCommand.Create<Unit, Unit>(n =>
                _teensySerial.PingDevice(), outputScheduler: ImmediateScheduler.Instance);

            this.WhenAnyValue(x => x.SelectedPort)
                .Where(port => port != null)
                .Subscribe(port => _teensySerial.SetPort(port));

            _teensySerial.Logs.Subscribe(log => 
            {
                _logBuilder.AppendLine(log);
                Logs = _logBuilder.ToString();
            });

            SelectedPort = Ports is not null && Ports?.Length == 0
                ? SelectedPort = ""
                : Ports![0];
        }
    }
}
