using RadEndpoints;
using TeensyRom.Api.Endpoints.ConnectDevice;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Device;

namespace TeensyRom.Api.Endpoints.ClosePort
{
    public class DisconnectDeviceRequest 
    {
        [FromRoute]
        public string DeviceId { get; set; } = string.Empty;
    }

    public class DisconnectDeviceRequestValidator : AbstractValidator<DisconnectDeviceRequest>
    {
        public DisconnectDeviceRequestValidator()
        {
            RuleFor(x => x.DeviceId)
                .NotEmpty().WithMessage("Device ID is required.")
                .Must(deviceId => deviceId.IsValidFilenameSafeHash()).WithMessage("Device ID must be a valid filename-safe hash of 8 characters long.");
        }
    }

    public class DisconnectDeviceResponse
    {
        public string Message { get; set; } = "Success!";
    }
}