using MediatR;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Common;
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
        private ISerialStateContext _singleDeviceContext;
        private ISerialStateContext _serial;
        private readonly IDeviceConnectionManager _deviceManager;

        public SerialBehavior(ISerialStateContext serial, IDeviceConnectionManager deviceManager)
        {
            _serial = serial;
            _singleDeviceContext = serial;
            _deviceManager = deviceManager;
        }
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            TResponse response = default!;

            BindSerial(request);

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

        public void BindSerial(TRequest request) 
        {
            if (request is ITeensyCommand<TResponse> command)
            {
                if (command.DeviceId is null) 
                {
                    command.Serial = _singleDeviceContext;
                    return;
                }

                var device = _deviceManager.GetConnectedDevice(command.DeviceId);

                if (device is null)
                {
                    throw new TeensyException($"Failed to find device with ID: {command.DeviceId}");
                }
                _serial = device.SerialState;
                command.Serial = device.SerialState;
            }
            else 
            {
                _serial = _singleDeviceContext;
            }
        }
    }
}