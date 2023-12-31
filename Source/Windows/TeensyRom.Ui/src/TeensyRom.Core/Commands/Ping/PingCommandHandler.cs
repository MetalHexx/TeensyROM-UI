using MediatR;
using TeensyRom.Core.Common;
using TeensyRom.Core.Serial;

namespace TeensyRom.Core.Commands
{
    public class PingCommandHandler(IObservableSerialPort _serialPort) : IRequestHandler<PingCommand, PingResult>
    {
        public Task<PingResult> Handle(PingCommand request, CancellationToken cancellationToken)
        {
            if (!_serialPort.IsOpen)
            {
                throw new TeensyException("You must first connect in order to ping the device.");
            }
            _serialPort.Write(TeensyByteToken.Ping_Bytes.ToArray(), 0, 2);

            return Task.FromResult(new PingResult());
        }
    }
}
