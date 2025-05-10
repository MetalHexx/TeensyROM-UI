using MediatR;
using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Serial;

namespace TeensyRom.Core.Commands
{
    public class CopyFileHandler(ISerialStateContext serialState) : IRequestHandler<CopyFileCommand, CopyFileResult>
    {
        public Task<CopyFileResult> Handle(CopyFileCommand request, CancellationToken cancellationToken)
        {
            serialState.SendIntBytes(TeensyToken.CopyFile, 2);
            serialState.HandleAck();
            serialState.SendIntBytes(request.StorageType.GetStorageToken(), 1);
            serialState.Write($"{request.SourcePath}\0");
            serialState.Write($"{request.DestPath}\0");
            serialState.HandleAck();
            return Task.FromResult(new CopyFileResult());
        }
    }
}