using MediatR;
using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Core.Commands.File.LaunchFile
{
    public class LaunchFileCommand(TeensyStorageType storageType, ILaunchableItem item) : IRequest<LaunchFileResult>
    {
        public string Path => LaunchItem.Path;
        public long Size => LaunchItem.Size;
        public TeensyStorageType StorageType { get; } = storageType;
        public ILaunchableItem LaunchItem { get; } = item;
    }
}