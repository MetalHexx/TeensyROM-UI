using MediatR;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TeensyRom.Ui.Core.Commands.SendString;
using TeensyRom.Core.Serial.State;

namespace TeensyRom.Ui.Features.Terminal.SerialCommand
{
    public interface ISerialCommandViewModel
    {
        ReactiveCommand<Unit, Unit> SendSerialCommand { get; set; }
        string SerialString { get; set; }
    }

    public class SerialCommandViewModel : ReactiveObject, ISerialCommandViewModel
    {
        [Reactive] public string SerialString { get; set; } = string.Empty;
        public ReactiveCommand<Unit, Unit> SendSerialCommand { get; set; }

        public SerialCommandViewModel(ISerialStateContext serial, IMediator mediator)
        {
            SendSerialCommand = ReactiveCommand.CreateFromTask
            (
                execute: async () =>
                {
                    var _ = await mediator.Send(new SendStringCommand(SerialString));
                    SerialString = string.Empty;
                    return Unit.Default;
                },
                canExecute: serial.CurrentState
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Select(state => state is SerialConnectedState)
                    .DistinctUntilChanged(),

                outputScheduler: RxApp.MainThreadScheduler
            );
        }
    }
}
