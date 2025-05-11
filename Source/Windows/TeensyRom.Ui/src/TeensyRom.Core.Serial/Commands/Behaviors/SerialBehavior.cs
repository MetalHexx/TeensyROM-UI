using MediatR;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using TeensyRom.Core.Abstractions;
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
        private ISerialStateContext _serial;
        private readonly IDeviceConnectionManager _deviceManager;

        public SerialBehavior(ISerialStateContext serial, IDeviceConnectionManager deviceManager)
        {
            _serial = serial;
            _deviceManager = deviceManager;
        }
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            TResponse response = default!;

            var serial = BindSerial(request);
            
            await serial.CurrentState
                .Where(state => state is not SerialBusyState)
                .FirstAsync()
                .ToTask(cancellationToken);

            try
            {
                serial.Lock();
                serial.StopHealthCheck();
                serial.TransitionTo(typeof(SerialBusyState));
                response = await next();
            }
            finally
            {
                serial.Unlock();
                serial.StartHealthCheck();
            }
            return response;
        }

        public ISerialStateContext BindSerial(TRequest request) 
        {
            if (request is ITeensyCommand<TResponse> command)
            {
                if (command.Serial is not null) 
                {
                    return command.Serial;
                }
                if (command.DeviceId is null) 
                {
                    command.Serial = _serial;
                    return _serial;
                }
                var device = _deviceManager.GetConnectedDevice(command.DeviceId);

                if (device is not null)
                {
                    command.Serial = device.SerialState;
                    return device.SerialState;
                }
            }
            return _serial;
        }
    }
}