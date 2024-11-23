using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Cli.Core.Commands.SendString;
using TeensyRom.Cli.Core.Serial;

namespace TeensyRom.Cli.Core.Commands.SendString
{
    public record SendStringCommand(string StringToSend) : IRequest<SendStringResult>;
}