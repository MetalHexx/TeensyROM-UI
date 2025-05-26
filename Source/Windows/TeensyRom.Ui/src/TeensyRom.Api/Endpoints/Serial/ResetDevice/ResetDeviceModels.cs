using RadEndpoints;
using TeensyRom.Core.Common;

namespace TeensyRom.Api.Endpoints.ResetDevice
{
    /// <summary>
    /// Request model for resetting a TeensyROM device.
    /// </summary>
    public class ResetDeviceRequest 
    {
        /// <summary>
        /// The unique ID of the TeensyROM device.
        /// </summary>
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

    /// <summary>
    /// Response model for the result of a reset device operation.
    /// </summary>
    public class ResetDeviceResponse
    {
        /// <summary>
        /// A message indicating the result of the operation.
        /// </summary>
        public string Message { get; set; } = "Success!";
    }
}