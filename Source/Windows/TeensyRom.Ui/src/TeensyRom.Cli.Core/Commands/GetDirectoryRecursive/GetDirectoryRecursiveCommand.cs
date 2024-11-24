using MediatR;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Cli.Core.Commands
{
    public class GetDirectoryRecursiveCommand(TeensyStorageType storageType, string path) : IRequest<GetDirectoryRecursiveResult>
    {
        public TeensyStorageType StorageType { get; } = storageType;
        public string Path { get; } = path;
    }
}
