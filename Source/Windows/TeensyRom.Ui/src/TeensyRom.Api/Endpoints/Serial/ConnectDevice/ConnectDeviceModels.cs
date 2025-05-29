using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TeensyRom.Api.Models;
using TeensyRom.Core.Entities.Device;

namespace TeensyRom.Api.Endpoints.ConnectDevice
{
    /// <summary>
    /// Request model for connecting to a TeensyROM device.
    /// </summary>
    public class ConnectDeviceRequest 
    {
        /// <summary>
        /// The unique ID of the TeensyROM device.
        /// </summary>
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

    /// <summary>
    /// Response model for the result of a connect device operation.
    /// </summary>
    public class ConnectDeviceResponse : ApiResponse
    {
        /// <summary>
        /// The connected TeensyROM cartridge information.
        /// </summary>
        [Required] public CartDto ConnectedCart { get; set; } = null!;

        /// <summary>
        /// A message indicating the result of the operation.
        /// </summary>
        [Required] public override string Message { get; set; } = "Connection successful!";
    }
}