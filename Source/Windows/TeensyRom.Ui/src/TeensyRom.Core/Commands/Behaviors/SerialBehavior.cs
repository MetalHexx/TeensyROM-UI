using MediatR;
using System.IO.Ports;
using System.Threading;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Common;
using TeensyRom.Core.Serial;

/// <summary>
/// Disables the serial read auto-poll behavior for the duration of the command and reneables it after.
/// </summary>
public class SerialBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : CommandResult, new()
{
    private static readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly IObservableSerialPort _serialPort;
    private bool _isBusy = false;

    public SerialBehavior(IObservableSerialPort serialPort)
    {
        _serialPort = serialPort;
        _serialPort.IsBusy.Subscribe(isBusy => _isBusy = isBusy);
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            //if (_isBusy)
            //{
            //    throw new TeensyException("Serial port is busy. Slow down and wait for the current command to complete. :)");
            //}

            _serialPort.DisableAutoReadStream();

            var response = await next();

            return response;
        }
        finally
        {
            _serialPort.EnableAutoReadStream();
            _semaphore.Release();
        }
    }
}