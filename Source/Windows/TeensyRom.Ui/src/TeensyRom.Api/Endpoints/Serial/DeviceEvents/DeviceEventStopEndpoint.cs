namespace TeensyRom.Api.Endpoints.GetDeviceEvents
{
    public class DeviceStopResponse
    {
        public string Message { get; set; } = "Success!";
    }
    public class DeviceEventStopEndpoint(IDeviceEventStream deviceEventStream) : RadEndpointWithoutRequest<DeviceStopResponse>
    {
        public override void Configure()
        {
            Delete("/devices/events")
                .WithName("StopDeviceEvents")
                .WithSummary("Stop Device Event Stream")
                .WithTags("Devices")
                .WithDescription("Stops the device event stream.")
                .Produces<DeviceStopResponse>();
        }
        public override Task Handle(CancellationToken ct)
        {
            deviceEventStream.Stop();
            Response = new DeviceStopResponse();
            Send();
            return Task.CompletedTask;
        }
    }
}