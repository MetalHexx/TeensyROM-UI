using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using RadEndpoints;
//using System.Net.ServerSentEvents;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using TeensyRom.Api.Endpoints.Serial.GetLogs;
using TeensyRom.Api.Http;
using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Entities.Device;

namespace TeensyRom.Api.Endpoints.GetDeviceEvents
{
    public class DeviceEventDto
    {
        public string DeviceId { get; set; } = string.Empty;
        public DeviceState State { get; set; }
    }
    public class GetDeviceEventsEndpoint(IDeviceConnectionManager deviceManager) : RadEndpoint
    {
        private static readonly IScheduler _eventScheduler = new EventLoopScheduler();

        public override void Configure()
        {
            RouteBuilder
                .MapGet("/device/events", (CancellationToken c) => Handle(c))
                .ExcludeFromDescription();
        }

        public IResult Handle(CancellationToken ct)
        {
            //var channel = Channel.CreateUnbounded<SseItem<DeviceEventDto>>();

            //var deviceEventObservable = deviceManager.DeviceStateChanges
            //    .SubscribeOn(_eventScheduler)
            //    .ObserveOn(_eventScheduler)
            //    .Where(x => x is not null)
            //    .Select(x => new DeviceEventDto
            //    {
            //        DeviceId = x!.DeviceId,
            //        State = CartDto.FromSerialState(x.State)
            //    })
            //    .Select(deviceEvent => new SseItem<DeviceEventDto>(deviceEvent, "device-event")
            //    {
            //        ReconnectionInterval = TimeSpan.FromMinutes(1)
            //    });
         
            //var events = channel.WriteObservableToChannel(deviceEventObservable, ct);

            //return TypedResults.ServerSentEvents(events);

            return TypedResults.Ok("Success");
        }        
    }
}