using MediatR;
using TeensyRom.Core.Common;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.State;

namespace TeensyRom.Core.Commands
{
    public class ResetCommandHandler(ISerialStateContext _serialState) : IRequestHandler<ResetCommand, ResetResult>
    {
        public Task<ResetResult> Handle(ResetCommand request, CancellationToken cancellationToken)
        {
            _serialState.Write(TeensyByteToken.Reset_Bytes.ToArray(), 0, 2);
            return Task.FromResult(new ResetResult());
        }
    }
}
