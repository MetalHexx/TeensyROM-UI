using MediatR;
using System.Collections.Generic;
using System;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Reactive;
using TeensyRom.Ui.Core.Logging;
using TeensyRom.Ui.Core.Settings;

namespace TeensyRom.Ui.Core.Commands
{
    public class PingCommand : IRequest<PingResult> { }
}
