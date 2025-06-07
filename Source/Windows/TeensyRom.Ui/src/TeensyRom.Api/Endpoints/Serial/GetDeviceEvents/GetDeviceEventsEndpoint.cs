using RadEndpoints;
using TeensyRom.Core.Abstractions;

namespace TeensyRom.Api.Endpoints.GetDeviceEvents
{
    public class DeviceEventDto
    {
        public string DeviceId { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
    public class GetDeviceEventsEndpoint(IDeviceConnectionManager deviceManager) : RadEndpoint<GetDeviceEventsRequest, GetDeviceEventsResponse>
    {
        public override void Configure()
        {
            Get("/devices/events")
                .Produces<GetDeviceEventsResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithDocument(tag: "Serial", desc: "Get device events");
        }

        public override async Task Handle(GetDeviceEventsRequest r, CancellationToken ct)
        {
            var devices = deviceManager.GetConnectedDevices();


            Response = new();
            Send();
        }
    }
}