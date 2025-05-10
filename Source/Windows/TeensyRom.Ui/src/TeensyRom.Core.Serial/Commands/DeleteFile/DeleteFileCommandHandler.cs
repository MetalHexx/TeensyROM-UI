using MediatR;
using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Serial;

namespace TeensyRom.Core.Commands.DeleteFile
{
    public class DeleteFileCommandHandler(ISerialStateContext serialState) : IRequestHandler<DeleteFileCommand, DeleteFileResult>
    {
        public Task<DeleteFileResult> Handle(DeleteFileCommand r, CancellationToken cancellationToken)
        {
            try
            {
                serialState.SendIntBytes(TeensyToken.DeleteFile, 2);

                serialState.HandleAck();
                serialState.SendIntBytes(r.StorageType.GetStorageToken(), 1);
                serialState.Write($"{r.Path}\0");
                serialState.HandleAck();

                return Task.FromResult(new DeleteFileResult());
            }
            catch (Exception ex)
            {
                return Task.FromResult(new DeleteFileResult
                {
                    Error = ex.ToString(),
                    IsSuccess = false
                });
            }
        }
    }
}
