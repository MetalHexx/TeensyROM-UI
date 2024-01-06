using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Common;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands.DeleteFile
{
    public class DeleteFileCommand : IRequest<DeleteFileResult>
    {
        public string Path{ get; init; } = string.Empty;
        public TeensyStorageType StorageType { get; set; }
    }

    public class DeleteFileResult : CommandResult { }

    public class DeleteFileCommandHandler : IRequestHandler<DeleteFileCommand, DeleteFileResult>
    {
        private readonly IObservableSerialPort _serialPort;

        public DeleteFileCommandHandler(IObservableSerialPort serialPort)
        {
            _serialPort = serialPort;
        }

        public Task<DeleteFileResult> Handle(DeleteFileCommand r, CancellationToken cancellationToken)
        {
            _serialPort.SendIntBytes(TeensyToken.DeleteFile, 2);

            _serialPort.HandleAck();
            _serialPort.SendIntBytes(r.StorageType.GetStorageToken(), 1);
            _serialPort.Write($"{r.Path}\0");
            _serialPort.HandleAck();

            return Task.FromResult(new DeleteFileResult());
        }
    }
}
