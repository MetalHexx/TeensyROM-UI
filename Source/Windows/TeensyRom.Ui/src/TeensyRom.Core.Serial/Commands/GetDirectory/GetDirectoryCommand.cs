using MediatR;
using System.IO;
using System.IO.Ports;
using System.Runtime;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Core.Commands
{
    public class GetDirectoryCommand(TeensyStorageType storageType, string path) : IRequest<GetDirectoryResult>
    {
        public TeensyStorageType StorageType { get; } = storageType;
        public string Path { get; } = path;
    }
}
