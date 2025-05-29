using System.ComponentModel.DataAnnotations;
using TeensyRom.Api.Endpoints.ConnectDevice;
using TeensyRom.Api.Models;
using TeensyRom.Core.Entities.Device;

namespace TeensyRom.Api.Endpoints.FindCarts
{
    /// <summary>
    /// Response model for finding available and connected TeensyROM devices.
    /// </summary>
    public class FindDevicesResponse
    {
        /// <summary>
        /// The list of TeensyROM devices that are available to connect.
        /// </summary>
        [Required] public List<CartDto> AvailableCarts { get; set; } = [];

        /// <summary>
        /// The list of TeensyROM devices that are currently connected.
        /// </summary>
        [Required] public List<CartDto> ConnectedCarts { get; set; } = [];

        /// <summary>
        /// A message indicating the result of the operation.
        /// </summary>
        [Required] public string Message { get; set; } = "Success!";
    }
}