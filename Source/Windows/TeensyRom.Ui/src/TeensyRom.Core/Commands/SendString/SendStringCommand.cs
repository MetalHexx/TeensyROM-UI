using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Commands.SendString;
using TeensyRom.Core.Serial;

namespace TeensyRom.Core.Commands.SendString
{
    public record SendStringCommand(string StringToSend) : IRequest<SendStringResult>;
}