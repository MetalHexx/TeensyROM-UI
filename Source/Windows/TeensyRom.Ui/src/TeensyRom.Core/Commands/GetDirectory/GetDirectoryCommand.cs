using MediatR;
using System.IO;
using System.IO.Ports;
using System.Runtime;
using TeensyRom.Core.Logging;

namespace TeensyRom.Core.Commands
{
    public class GetDirectoryCommand : IRequest<GetDirectoryResponse> 
    {
        public string Path { get; init; } = string.Empty;
        public uint Skip { get; init; }
        public uint Take { get; init; }
    }
}
