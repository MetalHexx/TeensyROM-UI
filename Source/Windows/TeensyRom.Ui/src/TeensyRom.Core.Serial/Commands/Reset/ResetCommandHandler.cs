using MediatR;
using System.Runtime.CompilerServices;
using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial.State;

namespace TeensyRom.Core.Commands
{
    public class ResetCommandHandler(IAlertService alert) : IRequestHandler<ResetCommand, ResetResult>
    {
        private ISerialStateContext _serialState = null!;
        
        public async Task<ResetResult> Handle(ResetCommand request, CancellationToken cancellationToken)
        {
            _serialState = request.Serial;
            var resetRoutine = new ResetSerialRoutine(_serialState, alert);
            var resetResult = await resetRoutine.Execute();

            return resetResult
                ? new ResetResult()
                : new ResetResult 
                {
                    IsSuccess = false,
                    Error = "Failed to reset device.  Check to make sure you're using the correct com port."
                };
        }
    }
}
