using MediatR;
using System.IO;
using System.IO.Ports;
using System.Runtime;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands
{
    public class GetDirectoryCommand : IRequest<GetDirectoryResult>
    {
        public TeensyStorageType StorageType { get; init; }
        public string Path { get; init; } = string.Empty;
    }
}
