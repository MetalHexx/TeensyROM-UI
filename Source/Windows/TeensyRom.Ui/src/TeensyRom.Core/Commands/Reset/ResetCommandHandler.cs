using MediatR;
using TeensyRom.Core.Commands.File.LaunchFile;
using TeensyRom.Core.Common;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.State;

namespace TeensyRom.Core.Commands
{
    public class ResetCommandHandler(ISerialStateContext _serialState) : IRequestHandler<ResetCommand, ResetResult>
    {
        public async Task<ResetResult> Handle(ResetCommand request, CancellationToken cancellationToken)
        {
            _serialState.Write(TeensyByteToken.Reset_Bytes.ToArray(), 0, 2);

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
            for (int i = 0; i < 10; i++)
            {
                var response = _serialState.ReadAndLogSerialAsString(1000);
                if (response.Contains("Resetting C64"))
                {
                    return true;
                }
                await Task.Delay(1000);
            }
            return false;
        }
    }
}
