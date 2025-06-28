namespace TeensyRom.Api.Endpoints.GetDeviceEvents
{
    public class DeviceStartResponse
    {
        public string Message { get; set; } = "Success!";
    }
    public class DeviceEventStartEndpoint(IDeviceEventStream deviceEventStream) : RadEndpointWithoutRequest<DeviceStartResponse>
    {
        public override void Configure()
        {
            Get("/devices/events")
                .WithName("StartDeviceEvents")
                .WithSummary("Start Device Event Stream")
                .WithTags("Devices")
                .WithDescription("Starts the device event stream.")
                .Produces<DeviceStartResponse>();
        }
        public override Task Handle(CancellationToken ct)
        {
            deviceEventStream.Start();
            Response = new DeviceStartResponse();
            Send();
            return Task.CompletedTask;
        }
    }
}