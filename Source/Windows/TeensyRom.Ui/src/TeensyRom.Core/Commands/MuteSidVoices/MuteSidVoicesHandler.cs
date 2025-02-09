using MediatR;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Settings;

namespace TeensyRom.Core.Commands.MuteSidVoices
{
    public class MuteSidVoicesHandler : IRequestHandler<MuteSidVoicesCommand, MuteSidVoicesResult>
    {
        private readonly ISerialStateContext _serialState;

        public MuteSidVoicesHandler(ISerialStateContext serialState, ISettingsService settings)
        {
            _serialState = serialState;
        }

        public Task<MuteSidVoicesResult> Handle(MuteSidVoicesCommand request, CancellationToken cancellationToken)
        {
            _serialState.SendIntBytes(TeensyToken.SIDVoiceMuting, 2);
            _serialState.SendSignedChar((sbyte)request.VoiceMuteInfo);
            var ack = _serialState.HandleAck();

            return Task.FromResult(new MuteSidVoicesResult());
        }
    }
}
