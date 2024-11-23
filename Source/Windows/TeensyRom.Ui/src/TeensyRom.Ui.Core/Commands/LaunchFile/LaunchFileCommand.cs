using MediatR;
using System.IO;
using System.Reactive;
using TeensyRom.Ui.Core.Logging;
using TeensyRom.Ui.Core.Storage.Entities;

namespace TeensyRom.Ui.Core.Commands.File.LaunchFile
{
    public class LaunchFileCommand(TeensyStorageType storageType, ILaunchableItem item) : IRequest<LaunchFileResult>
    {
        public string Path => LaunchItem.Path;
        public long Size => LaunchItem.Size;
        public TeensyStorageType StorageType { get; } = storageType;
        public ILaunchableItem LaunchItem { get; } = item;
    }
}