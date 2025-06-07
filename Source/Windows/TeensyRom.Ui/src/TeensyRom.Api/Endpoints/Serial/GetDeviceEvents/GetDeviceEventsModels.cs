using RadEndpoints;

namespace TeensyRom.Api.Endpoints.GetDeviceEvents
{
    public class GetDeviceEventsRequest { }

    public class GetDeviceEventsRequestValidator : AbstractValidator<GetDeviceEventsRequest>
    {
        public GetDeviceEventsRequestValidator()
        {
            //TODO: Add validation rules here
        }
    }

    public class GetDeviceEventsResponse
    {
        public string Message { get; set; } = "Success!";
    }
}