using TeensyRom.Core.Entities.Device;

namespace TeensyRom.Api.Endpoints.FindCarts
{
    public class FindCartsResponse
    {
        public List<Cart> AvailableCarts { get; set; } = [];
        public List<Cart> ConnectedCarts { get; set; } = [];
        public string Message { get; set; } = "Success!";
    }
}