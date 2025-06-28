using Microsoft.AspNetCore.SignalR;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using TeensyRom.Api.Services;
using TeensyRom.Core.Abstractions;

namespace TeensyRom.Api.Endpoints.GetDeviceEvents
{   
    public class DeviceEventDto
    {
        public string DeviceId { get; set; } = string.Empty;
        public DeviceState State { get; set; }
    }
    public class DeviceEventHub : Hub { }

    public interface IDeviceEventStream
    {
        void Start();
        void Stop();
    }

    public class DeviceEventStream(IHubContext<DeviceEventHub> hubContext, IDeviceConnectionManager deviceManager) : IDeviceEventStream
    {
        private static readonly IScheduler _eventScheduler = new EventLoopScheduler();
        private readonly IHubContext<DeviceEventHub> _hubContext = hubContext;
        private IDisposable? _deviceEventSubscription;

        public void Start()
        {
            _deviceEventSubscription = deviceManager.DeviceStateChanges
              .SubscribeOn(_eventScheduler)
              .ObserveOn(_eventScheduler)
              .Where(x => x is not null)
              .Select(x => new DeviceEventDto
              {
                  DeviceId = x!.DeviceId,
                  State = CartDto.FromSerialState(x.State)
              })
              .Subscribe(async deviceEvent =>
              {
                  await _hubContext.Clients.All.SendAsync("DeviceEvent", deviceEvent);
              });
        }

        public void Stop()
        {
            _deviceEventSubscription?.Dispose();
            _deviceEventSubscription = null;
        }
    }
}