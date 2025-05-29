using RadEndpoints;
using System.ComponentModel.DataAnnotations;
using TeensyRom.Api.Endpoints.ConnectDevice;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Device;

namespace TeensyRom.Api.Endpoints.ClosePort
{
    /// <summary>
    /// Request model for disconnecting from a TeensyROM device.
    /// </summary>
    public class DisconnectDeviceRequest
    {
        /// <summary>
        /// The unique ID of the TeensyROM device.
        /// </summary>
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

    /// <summary>
    /// Response model for the result of a disconnect device operation.
    /// </summary>
    public class DisconnectDeviceResponse
    {
        /// <summary>
        /// A message indicating the result of the operation.
        /// </summary>
        [Required] public string Message { get; set; } = "Success!";
    }
}