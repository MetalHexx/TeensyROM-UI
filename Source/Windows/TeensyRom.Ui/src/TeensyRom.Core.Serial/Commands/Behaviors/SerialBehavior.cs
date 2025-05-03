using MediatR;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Serial.State;

namespace TeensyRom.Core.Serial.Commands.Behaviors 
{
    /// <summary>
    /// Disables the serial read auto-poll behavior for the duration of the command and reneables it after.
    /// </summary>
    public class SerialBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : TeensyCommandResult, new()
    {
        private readonly ISerialStateContext _serial;

        public SerialBehavior(ISerialStateContext serial)
        {
            _serial = serial;
        }
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            TResponse response = default!;

            await _serial.CurrentState
                .Where(state => state is not SerialBusyState)
                .FirstAsync()
                .ToTask(cancellationToken);

            try
            {
                _serial.Lock();
                _serial.StopHealthCheck();
                _serial.TransitionTo(typeof(SerialBusyState));
                response = await next();
            }
            finally
            {
                _serial.Unlock();
                _serial.StartHealthCheck();
            }
            return response;
        }
    }
}