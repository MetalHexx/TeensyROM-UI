using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Common;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands.DeleteFile
{
    public class DeleteFileCommand : IRequest<DeleteFileResult>
    {
        public string Path{ get; init; } = string.Empty;
        public TeensyStorageType StorageType { get; set; }
    }

    public class DeleteFileResult : TeensyCommandResult { }

    public class DeleteFileCommandHandler : IRequestHandler<DeleteFileCommand, DeleteFileResult>
    {
        private readonly ISerialStateContext _serialState;

        public DeleteFileCommandHandler(ISerialStateContext serialState)
        {
            _serialState = serialState;
        }

        public Task<DeleteFileResult> Handle(DeleteFileCommand r, CancellationToken cancellationToken)
        {
            _serialState.SendIntBytes(TeensyToken.DeleteFile, 2);

            _serialState.HandleAck();
            _serialState.SendIntBytes(r.StorageType.GetStorageToken(), 1);
            _serialState.Write($"{r.Path}\0");
            _serialState.HandleAck();

            return Task.FromResult(new DeleteFileResult());
        }
    }
}
