using RadEndpoints;
using TeensyRom.Core.Common;

namespace TeensyRom.Api.Endpoints.ClosePort
{
    public class ClosePortRequest 
    {
        [FromRoute]
        public string DeviceId { get; set; } = string.Empty;
    }

    public class ClosePortRequestValidator : AbstractValidator<ClosePortRequest>
    {
        public ClosePortRequestValidator()
        {
            RuleFor(x => x.DeviceId)
                .NotEmpty().WithMessage("Device ID is required.")
                .Must(deviceId => deviceId.IsValidFilenameSafeHash()).WithMessage("Device ID must be a valid filename-safe hash of 8 characters long.");
        }
    }

    public class ClosePortResponse
    {
        public string Message { get; set; } = "Success!";
    }
}