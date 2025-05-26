using TeensyRom.Api.Endpoints.ConnectDevice;
using TeensyRom.Api.Models;
using TeensyRom.Core.Entities.Device;

namespace TeensyRom.Api.Endpoints.FindCarts
{
    public class FindDevicesResponse
    {
        public List<CartDto> AvailableCarts { get; set; } = [];
        public List<CartDto> ConnectedCarts { get; set; } = [];
        public string Message { get; set; } = "Success!";
    }
}