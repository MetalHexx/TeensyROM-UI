using MediatR;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Common;

namespace TeensyRom.Core.Commands.MuteSidVoices
{
    public class MuteSidVoicesHandler : IRequestHandler<MuteSidVoicesCommand, MuteSidVoicesResult>
    {
        private readonly ISerialStateContext _serialState;

        public MuteSidVoicesHandler(ISerialStateContext serialState, ISettingsService settings)
        {
            _serialState = serialState;
        }

        public async Task<MuteSidVoicesResult> Handle(MuteSidVoicesCommand request, CancellationToken cancellationToken)
        {
            var attemptNumber = 1;

            while (attemptNumber <= 3)
            {
                try
                {
                    _serialState.SendIntBytes(TeensyToken.SIDVoiceMuting, 2);
                    _serialState.SendSignedChar((sbyte)request.VoiceMuteInfo);
                    var ack = _serialState.HandleAck();
                    break;
                }
                catch (TeensyException)
                {
                    await Task.Delay(attemptNumber * 100);

                    if (attemptNumber == 3)
                    {
                        throw new TeensyDjException();
                    }
                    attemptNumber++;
                    continue;
                }
            }
            return new MuteSidVoicesResult();
        }
    }
}
