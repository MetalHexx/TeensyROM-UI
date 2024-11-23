using MediatR;
using System.IO;
using System.IO.Ports;
using System.Runtime;
using TeensyRom.Ui.Core.Logging;
using TeensyRom.Ui.Core.Storage.Entities;

namespace TeensyRom.Ui.Core.Commands
{
    public class GetDirectoryCommand(TeensyStorageType storageType, string path) : IRequest<GetDirectoryResult>
    {
        public TeensyStorageType StorageType { get; } = storageType;
        public string Path { get; } = path;
    }
}
