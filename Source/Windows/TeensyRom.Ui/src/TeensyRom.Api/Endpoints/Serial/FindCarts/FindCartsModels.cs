using RadEndpoints;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Settings;

namespace TeensyRom.Api.Endpoints.FindCarts
{
    public class FindCartsResponse
    {
        public List<Cart> AvailableCarts { get; set; } = [];
        public List<Cart> ConnectedCarts { get; set; } = [];
        public string Message { get; set; } = "Success!";
    }
}