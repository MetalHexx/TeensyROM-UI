using MediatR;
using System.IO;
using System.Reactive;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands
{
    public class QueuedSaveFileCommand : IRequest<QueuedSaveFileResult>, IQueuedTeensyCommand
    {
        public TeensyFileInfo File { get; init; } = default!;
    }
}