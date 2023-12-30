using MediatR;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Serial;

/// <summary>
/// Disables the serial read auto-poll behavior for the duration of the command and reneables it after.
/// </summary>
public class DisableSerialPollBehavior<TRequest, TResponse>(IObservableSerialPort serialPort) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : CommandResult, new()
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        serialPort.DisableAutoReadStream();

        var response = await next();

        serialPort.EnableAutoReadStream();

        return response;
    }
}
