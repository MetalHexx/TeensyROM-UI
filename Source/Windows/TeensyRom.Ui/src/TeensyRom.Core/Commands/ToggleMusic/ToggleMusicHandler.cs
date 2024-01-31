using MediatR;
using TeensyRom.Core.Common;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.State;

namespace TeensyRom.Core.Commands
{
    public class ToggleMusicHandler(ISerialStateContext _serialState) :  IRequestHandler<ToggleMusicCommand, ToggleMusicResult>
    {
        public Task<ToggleMusicResult> Handle(ToggleMusicCommand request, CancellationToken cancellationToken)
        {
            _serialState.SendIntBytes(TeensyToken.PauseMusic, 2);

            try
            {
                _serialState.HandleAck();
            }
            catch (TeensyException ex)
            {
                if (ParseBusyResponse(ex.Message))
                {
                    return Task.FromResult(new ToggleMusicResult
                    {
                        IsSuccess = false,
                        IsBusy = true
                    });

                }
                throw;
            }
            return Task.FromResult(new ToggleMusicResult());
        }

        private bool ParseBusyResponse(string response) => response.Contains("Busy") ? true : false;
    }
}
