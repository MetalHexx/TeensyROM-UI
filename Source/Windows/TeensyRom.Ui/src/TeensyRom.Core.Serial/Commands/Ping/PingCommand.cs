using MediatR;
using System.Collections.Generic;
using System;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Reactive;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Serial.Commands;
using TeensyRom.Core.Abstractions;

namespace TeensyRom.Core.Commands
{
    public class PingCommand : ITeensyCommand<PingResult>
    {
        public string? DeviceId { get; set; }
        public ISerialStateContext Serial { get; set; } = null!;
    }
}
