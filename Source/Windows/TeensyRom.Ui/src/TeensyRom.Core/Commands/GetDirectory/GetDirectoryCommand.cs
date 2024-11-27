using MediatR;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands.GetDirectory
{
    public class GetDirectoryCommand(TeensyStorageType storageType, string path) : IRequest<GetDirectoryResult>
    {
        public TeensyStorageType StorageType { get; } = storageType;
        public string Path { get; } = path;
    }
}
