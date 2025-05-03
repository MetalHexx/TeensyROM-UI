using MediatR;
using System.Collections.Generic;
using System;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Reactive;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Settings;

namespace TeensyRom.Core.Commands
{
    public class PingCommand : IRequest<PingResult> { }
}
