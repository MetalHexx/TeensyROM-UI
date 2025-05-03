using MediatR;

namespace TeensyRom.Core.Commands.MuteSidVoices
{
    public class MuteSidVoicesHandler(IMuteSidVoicesSerialRoutine muteVoices) : IRequestHandler<MuteSidVoicesCommand, MuteSidVoicesResult>
    {
        public async Task<MuteSidVoicesResult> Handle(MuteSidVoicesCommand request, CancellationToken cancellationToken)
        {
            await muteVoices.Execute(request.Voice1Enabled, request.Voice2Enabled, request.Voice3Enabled);
            return new MuteSidVoicesResult();
        }
    }
}
