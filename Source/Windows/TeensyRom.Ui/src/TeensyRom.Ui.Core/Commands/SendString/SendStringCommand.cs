using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Ui.Core.Commands.SendString;
using TeensyRom.Ui.Core.Serial;

namespace TeensyRom.Ui.Core.Commands.SendString
{
    public record SendStringCommand(string StringToSend) : IRequest<SendStringResult>;
}