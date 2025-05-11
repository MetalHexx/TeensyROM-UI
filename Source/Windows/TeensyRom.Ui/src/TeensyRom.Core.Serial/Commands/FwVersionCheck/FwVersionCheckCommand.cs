using MediatR;
using System.Collections.Generic;
using System;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Reactive;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Abstractions;

namespace TeensyRom.Core.Serial.Commands.FwVersionCheck
{
    public class FwVersionCheckCommand : ITeensyCommand<FwVersionCheckResult>
    {
        public string? DeviceId { get; set; }
        public ISerialStateContext Serial { get; set; } = null!;
    }
}
