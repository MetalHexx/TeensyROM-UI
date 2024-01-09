using MediatR;
using System.IO;
using System.IO.Ports;
using System.Runtime;
using TeensyRom.Core.Logging;

namespace TeensyRom.Core.Commands
{
    public class GetDirectoryRecursiveCommand : IRequest<GetDirectoryRecursiveResult> 
    {
        public string Path { get; init; } = string.Empty;
    }
}
