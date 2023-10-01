using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Windows.Documents;
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

        //TODO: Add a command to connect to the selected serial port

        //TODO: Add a property to display the logs received from the serial port

        private readonly IObservableSerialPort _serialService;
        private readonly StringBuilder _logBuilder = new StringBuilder();

        public ConnectViewModel(IObservableSerialPort serialService)
        {
            _serialService = serialService;

            _serialService.Ports.ToPropertyEx(this, vm => vm.Ports);

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
