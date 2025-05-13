using RadEndpoints;
using TeensyRom.Core.Common;

namespace TeensyRom.Api.Endpoints.ResetDevice
{
    public class ResetDeviceRequest 
    {
        [FromRoute]
        public string DeviceId { get; set; } = string.Empty;
    }

    public class ResetDeviceRequestValidator : AbstractValidator<ResetDeviceRequest>
    {
        public ResetDeviceRequestValidator()
        {
            RuleFor(x => x.DeviceId)
                .NotEmpty().WithMessage("Device ID is required.")
                .Must(deviceId => deviceId.IsValidFilenameSafeHash()).WithMessage("Device ID must be a valid filename-safe hash of 8 characters long.");
        }
    }

    public class ResetDeviceResponse
    {
        public string Message { get; set; } = "Success!";
    }
}