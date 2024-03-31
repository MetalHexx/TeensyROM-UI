using MediatR;
using System.IO;
using System.Reactive;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands.File.LaunchFile
{
    public class LaunchFileCommand(TeensyStorageType storageType, string path) : IRequest<LaunchFileResult>
    {
        public TeensyStorageType StorageType { get; } = storageType;
        public string Path { get; } = path;
    }
}