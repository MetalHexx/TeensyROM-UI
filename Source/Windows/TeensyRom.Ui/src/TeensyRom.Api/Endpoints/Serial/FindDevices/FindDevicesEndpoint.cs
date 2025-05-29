using TeensyRom.Api.Models;
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
                .WithName("FindDevices")
                .WithSummary("Find Devices")
                .WithTags("Devices")
                .WithDescription(
                    "Returns all available and connected TeensyROM devices.\n\n" +
                    "- This will momentarily disconnect all devices.\n" +
                    "- All available COM ports will be scanned for TeensyROM devices.\n" +
                    "- All previously connected devices will reconnect."
                );
        }

        public override async Task Handle(CancellationToken ct)
        {
            var availableResults = await deviceManager.FindAvailableCarts();
            var connectedCarts = deviceManager.GetConnectedCarts();

            if (availableResults.Count == 0)
            {
                SendNotFound("No TeensyRom devices found.");
                return;
            }
            Response = new()
            {
                AvailableCarts = availableResults.Select(CartDto.FromCart).ToList(),
                ConnectedCarts = connectedCarts.Select(CartDto.FromCart).ToList(),
                Message = "Success!"
            };
            Send();
        }
    }
}