using Microsoft.AspNetCore.Mvc;
using TeensyRom.Core.Entities.Device;

namespace TeensyRom.Api.Endpoints.ConnectDevice
{
    public class ConnectDeviceRequest 
    {
        [FromRoute]
        public string DeviceId { get; set; } = string.Empty;
    }

    public class ConnectDeviceRequestValidator : AbstractValidator<ConnectDeviceRequest>
    {
        public ConnectDeviceRequestValidator()
        {
            RuleFor(x => x.DeviceId)
                .NotEmpty()
                .WithMessage("DeviceId cannot be empty.");
        }
    }

    public class ConnectDeviceResponse : ApiResponse
    {
        public CartDto ConnectedCart { get; set; } = null!;
        public override string Message { get; set; } = "Connection successful!";
    }
}