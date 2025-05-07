using RadEndpoints;
using System.Reactive.Linq;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.State;

namespace TeensyRom.Api.Endpoints.FindCarts
{
    public class FindCartsEndpoint(IDeviceConnectionManager deviceManager) : RadEndpointWithoutRequest<FindCartsResponse>
    {
        public override void Configure()
        {
            Get("/serial/carts")
                .Produces<FindCartsResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithDocument(tag: "Serial", desc: "Scans all the serial ports and attempts to identify ports that have a TeensyRom device.");
        }

        public override async Task Handle(CancellationToken ct)
        {
            var availableCarts = await deviceManager.FindAvailableCarts();
            var connectedCarts = deviceManager.GetConnectedCarts();

            if (availableCarts.Count == 0)
            {
                SendNotFound("No TeensyRom devices found.");
            }
            Response = new()
            {
                AvailableCarts = availableCarts,
                ConnectedCarts = connectedCarts,
                Message = "Success!"
            };
            Send();
        }
    }
}