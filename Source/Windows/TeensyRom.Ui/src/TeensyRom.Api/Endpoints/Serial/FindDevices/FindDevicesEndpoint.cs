using TeensyRom.Api.Http;
using TeensyRom.Api.Models;
using TeensyRom.Core.Abstractions;

namespace TeensyRom.Api.Endpoints.FindCarts
{
    public class FindDevicesEndpoint(IDeviceConnectionManager deviceManager) : RadEndpoint<FindDevicesRequest, FindDevicesResponse>
    {
        public override void Configure()
        {
            Get("/devices/")
                .Produces<FindDevicesResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status503ServiceUnavailable)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .RequireRateLimiting(RateLimitHelper.FindDevicesRateLimiter)
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

        public override async Task Handle(FindDevicesRequest r, CancellationToken ct)
        {
            var devices = await deviceManager.FindDevices(r.AutoConnectNew ?? false, ct);

            if (devices.Count == 0)
            {
                SendNotFound("No TeensyRom devices found.");
                return;
            }
            List<CartDto> deviceDtos = [.. await Task.WhenAll(devices.Select(CartDto.FromDevice))];

            Response = new()
            {
                Devices = deviceDtos.ToList(),
                Message = "Success!"
            };


            Response = new()
            {
                Devices = deviceDtos,
                Message = "Success!"
            };
            Send();
        }
    }
}