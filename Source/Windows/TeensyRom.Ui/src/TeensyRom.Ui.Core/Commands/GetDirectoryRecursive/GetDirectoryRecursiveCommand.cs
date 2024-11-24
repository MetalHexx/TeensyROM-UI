using MediatR;
using TeensyRom.Ui.Core.Storage.Entities;

namespace TeensyRom.Ui.Core.Commands
{
    public class GetDirectoryRecursiveCommand(TeensyStorageType storageType, string path) : IRequest<GetDirectoryRecursiveResult>
    {
        public TeensyStorageType StorageType { get; } = storageType;
        public string Path { get; } = path;
    }
}
