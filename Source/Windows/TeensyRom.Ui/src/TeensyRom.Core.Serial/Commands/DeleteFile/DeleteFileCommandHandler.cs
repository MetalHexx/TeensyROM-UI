using MediatR;
using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;

namespace TeensyRom.Core.Commands.DeleteFile
{
    public class DeleteFileCommandHandler(IAlertService alert) : IRequestHandler<DeleteFileCommand, DeleteFileResult>
    {
        public async Task<DeleteFileResult> Handle(DeleteFileCommand r, CancellationToken cancellationToken)
        {
            try
            {
                await ProcessDelete(r);
                return new DeleteFileResult();
            }
            catch (Exception ex)
            {
                alert.Publish("Delete Error: TR is busy.  Restarting TR.");

                return new DeleteFileResult
                {
                    Error = ex.ToString(),
                    IsSuccess = false,
                    IsBusy = ex.Message.Contains("busy")
                };
            }
        }

        private async Task<DeleteFileResult> ProcessDelete(DeleteFileCommand r)
        {
            try
            {
                Delete(r);
                return new DeleteFileResult();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("busy", StringComparison.OrdinalIgnoreCase)) 
                {
                    alert.Publish("Delete Error: TR is busy.  Restarting TR.");
                    var resetRoutine = new ResetSerialRoutine(r.Serial, alert);
                    var resetResult = await resetRoutine.Execute();

                    if (resetResult is true) 
                    {
                        Delete(r);
                        return new DeleteFileResult();
                    }
                    return new DeleteFileResult
                    {
                        IsSuccess = false,
                        Error = ex.ToString()
                    };
                }
                throw;
            }
        }

        private void Delete(DeleteFileCommand r) 
        {
            r.Serial.SendIntBytes(TeensyToken.DeleteFile, 2);
            r.Serial.HandleAck();
            r.Serial.SendIntBytes(r.StorageType.GetStorageToken(), 1);
            r.Serial.Write($"{r.Path}\0");
            r.Serial.HandleAck();
        }
    }
}
