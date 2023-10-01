using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive.Linq;
using TeensyRom.Core.Serial;

namespace TeensyRom.Ui.Features.Connect
{
    public class ConnectViewModel: ReactiveObject
    {
        [ObservableAsProperty]
        public string[]? Ports { get; }

        [Reactive]
        public string SelectedPort { get; set; }

        //TODO: Add a property to represent the selected port in the drop down

        //TODO: Add a command to set the serial port to the users selection

        //TODO: Add a command to connect to the selected serial port

        //TODO: Add a property to display the logs received from the serial port

        private readonly IObservableSerialPort _serialService;

        public ConnectViewModel(IObservableSerialPort serialService)
        {
            _serialService = serialService;
            _serialService.Ports.ToPropertyEx(this, vm => vm.Ports);

            this.WhenAnyValue(x => x.SelectedPort)
                .Where(port => port != null)
                .Subscribe(port => _serialService.SetPort(port));

            SelectedPort = Ports![0];
        }
    }
}
