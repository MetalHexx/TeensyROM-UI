using Microsoft.AspNetCore.Mvc;
using TeensyRom.Core.Entities.Device;

namespace TeensyRom.Api.Endpoints.ConnectDevice
{
    public class ConnectDeviceRequest 
    {
        [FromRoute]
        public string DeviceId { get; set; } = string.Empty;
    }

    public class OpenPortRequestValidator : AbstractValidator<ConnectDeviceRequest>
    {
        public OpenPortRequestValidator()
        {
            RuleFor(x => x.DeviceId)
                .NotEmpty()
                .WithMessage("DeviceId cannot be empty.");
        }
    }

    public class ConnectDeviceResponse : ApiResponse
    {
        public Cart ConnectedCart { get; set; } = null!;
        public override string Message { get; set; } = "Connection successful!";
    }
}