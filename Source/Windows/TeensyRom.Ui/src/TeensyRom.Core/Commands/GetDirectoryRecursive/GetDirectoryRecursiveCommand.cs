using MediatR;
using System.IO;
using System.IO.Ports;
using System.Runtime;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands
{
    public class GetDirectoryRecursiveCommand(TeensyStorageType storageType, string path) : IRequest<GetDirectoryRecursiveResult>
    {
        public TeensyStorageType StorageType { get; } = storageType;
        public string Path { get; } = path;
    }
}
