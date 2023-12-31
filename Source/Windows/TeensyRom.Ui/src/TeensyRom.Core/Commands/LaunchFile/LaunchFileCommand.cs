using MediatR;
using System.IO;
using System.Reactive;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands.File.LaunchFile
{
    public class LaunchFileCommand : IRequest<LaunchFileResponse>
    {
        public string Path { get; set; } = string.Empty;
    }
}