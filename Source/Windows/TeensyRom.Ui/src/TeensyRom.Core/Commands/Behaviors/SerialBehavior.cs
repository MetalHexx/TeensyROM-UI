using MediatR;
using System.IO.Ports;
using System.Reactive.Linq;
using System.Threading;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Common;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.State;

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
        var currentSerialState = await _serial.CurrentState.FirstAsync();

        if(currentSerialState is SerialBusyState)
        {
            throw new TeensyBusyException("TR is busy with your previous command.  Try again soon.");
        }
        try
        {
            _serial.Lock();
            _serial.StopHealthCheck();
            _serial.TransitionTo(typeof(SerialBusyState));                        
            response = await next();
        }
        catch (Exception ex)
        {
            _serial.Unlock();
            _serial.StartHealthCheck();
            throw;
        }
        _serial.Unlock();
        _serial.StartHealthCheck();

        return response;
    }
}