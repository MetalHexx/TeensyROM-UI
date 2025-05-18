using TeensyRom.Core.Abstractions;

namespace TeensyRom.Api.Endpoints.FindCarts
{
    public class FindDevicesEndpoint(IDeviceConnectionManager deviceManager) : RadEndpointWithoutRequest<FindDevicesResponse>
    {
        public override void Configure()
        {
            Get("/devices")
                .Produces<FindDevicesResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status503ServiceUnavailable)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithDocument(tag: "Devices", desc: "Returns all available and connected TeensyROM devices.");
        }

        public override async Task Handle(CancellationToken ct)
        {
            var findResults = await deviceManager.FindAvailableCarts();
            var connectedCarts = deviceManager.GetConnectedCarts();

            if (findResults.Count == 0)
            {
                SendNotFound("No TeensyRom devices found.");
                return;
            }
            Response = new()
            {
                AvailableCarts = findResults,
                ConnectedCarts = connectedCarts,
                Message = "Success!"
            };
            Send();
        }
    }
}