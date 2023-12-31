using MediatR;
using TeensyRom.Core.Common;
using TeensyRom.Core.Serial;

namespace TeensyRom.Core.Commands
{
    public class ResetCommandHandler(IObservableSerialPort _serialPort) : IRequestHandler<ResetCommand, ResetResult>
    {
        public Task<ResetResult> Handle(ResetCommand request, CancellationToken cancellationToken)
        {
            if (!_serialPort.IsOpen)
            {
                throw new TeensyException("You must first connect in order to reset the device.");
            }
            _serialPort.Write(TeensyByteToken.Reset_Bytes.ToArray(), 0, 2);
            return Task.FromResult(new ResetResult());
        }
    }
}
