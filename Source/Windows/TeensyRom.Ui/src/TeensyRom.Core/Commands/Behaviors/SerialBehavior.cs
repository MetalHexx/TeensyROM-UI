using MediatR;
using System.IO.Ports;
using System.Threading;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Commands.Behaviors;
using TeensyRom.Core.Common;
using TeensyRom.Core.Serial;

/// <summary>
/// Disables the serial read auto-poll behavior for the duration of the command and reneables it after.
/// </summary>
public class SerialBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : CommandResult, new()
{
    private readonly ITeensyCommandExecutor _executor;

    public SerialBehavior(IObservableSerialPort serialPort, ITeensyCommandExecutor executor)
    {
        _executor = executor;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        TResponse response = default!;

        await _executor.Execute(async () =>
        {
            response = await next();
        });
        return response;
    }
}