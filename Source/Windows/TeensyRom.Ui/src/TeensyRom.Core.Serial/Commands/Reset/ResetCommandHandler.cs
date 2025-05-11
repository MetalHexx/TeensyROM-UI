using MediatR;
using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Common;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;

namespace TeensyRom.Core.Commands
{
    public class ResetCommandHandler(IAlertService alert) : IRequestHandler<ResetCommand, ResetResult>
    {
        private ISerialStateContext _serialState = null!;
        
        public async Task<ResetResult> Handle(ResetCommand request, CancellationToken cancellationToken)
        {
            _serialState = request.Serial;
            //serialState.Write(TeensyByteToken.Reset_Bytes.ToArray(), 0, 2);
            _serialState.SendIntBytes(TeensyToken.Reset, 2);

            var pollResult = await PollForSuccess();
                     

            return pollResult
                ? new ResetResult()
                : new ResetResult 
                {
                    IsSuccess = false,
                    Error = "Failed to reset device.  Check to make sure you're using the correct com port."
                };
        }

        private async Task<bool> PollForSuccess()
        {
            var response = string.Empty;
            try
            {
                for (int i = 0; i < 10; i++)
                {
                    response = $"{response}{_serialState.ReadAndLogSerialAsString(1000)}";
                    if (response.Contains("Resetting C64"))
                    {
                        return true;
                    }
                    await Task.Delay(1000);
                }
            }
            catch (Exception ex)
            {
                if(ex.Message.Contains("port is closed"))
                {
                    alert.Publish("Disconnected from TeensyROM minimal mode.  Reconnecting.");
                    await HandleReconnection(1000);
                    return true;
                }
                throw;
            }
            return false;
        }

        private async Task HandleReconnection(int waitMs)
        {
            await Task.Delay(waitMs);

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    _serialState.EnsureConnection();
                    return;
                }
                catch (TeensyException)
                {
                    if (i == 2) throw;
                }
            }
        }
    }
}
