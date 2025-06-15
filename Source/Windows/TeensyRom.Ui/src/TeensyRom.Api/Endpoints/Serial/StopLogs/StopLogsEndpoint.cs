using TeensyRom.Api.Services;

namespace TeensyRom.Api.Endpoints.Serial.StopLogs
{
    public class StopLogsResponse
    {
        public string Message { get; set; } = "Success!";
    }

    public class StopLogsEndpoint(IQueuedChannelLogger logService) : RadEndpointWithoutRequest<StopLogsResponse>
    {
        public override void Configure()
        {
            Delete("/logs")
                .WithName("StopLogs")
                .WithSummary("Stop Logging Channel")
                .WithTags("Devices")
                .WithDescription("Stops the logging service and returns a success message.")
                .Produces<StopLogsResponse>();
        }
        public override async Task Handle(CancellationToken cancellationToken)
        {
            await logService.StopChannelQueue();
            Send();
        }
    }
}
