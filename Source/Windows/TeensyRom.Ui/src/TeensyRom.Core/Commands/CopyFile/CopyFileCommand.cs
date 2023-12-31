using MediatR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands
{
    public class CopyFileCommand : IRequest<CopyFileResult> 
    {
        public string SourcePath { get; init; } = string.Empty;
        public string DestPath { get; init; } = string.Empty;
    }
}