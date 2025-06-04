using System.ComponentModel.DataAnnotations;
using TeensyRom.Api.Endpoints.ConnectDevice;
using TeensyRom.Api.Models;
using TeensyRom.Core.Entities.Device;

namespace TeensyRom.Api.Endpoints.FindCarts
{
    public class FindDevicesRequest
    {
        [FromQuery] public bool? AutoConnectNew { get; set; }
    }
    /// <summary>
    /// Response model for finding available and connected TeensyROM devices.
    /// </summary>
    public class FindDevicesResponse
    {
        /// <summary>
        /// The list of TeensyROM devices that are available to connect.
        /// </summary>
        [Required] public List<CartDto> Devices { get; set; } = [];

        /// <summary>
        /// A message indicating the result of the operation.
        /// </summary>
        [Required] public string Message { get; set; } = "Success!";
    }
}