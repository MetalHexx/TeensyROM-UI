using RadEndpoints;
using TeensyRom.Core.Serial;

namespace TeensyRom.Api.Endpoints.FindCarts
{
    public class FindCartsResponse
    {
        public List<Cart> Carts { get; set; } = [];
        public string Message { get; set; } = "Success!";
    }
}