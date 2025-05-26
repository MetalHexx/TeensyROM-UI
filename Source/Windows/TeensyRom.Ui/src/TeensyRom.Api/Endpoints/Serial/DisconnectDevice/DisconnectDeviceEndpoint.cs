using RadEndpoints;
using TeensyRom.Core.Abstractions;

namespace TeensyRom.Api.Endpoints.ClosePort
{
    public class DisconnectDeviceEndpoint(IDeviceConnectionManager deviceManager) : RadEndpoint<DisconnectDeviceRequest, DisconnectDeviceResponse>
    {
        public override void Configure()
        {
            Delete("/devices/{deviceId}")
                .Produces<DisconnectDeviceResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithSummary("Disconnect Device")
                .WithTags("Devices")
                .WithDescription(
                    "Disconnnects from a TeensyROM device.\n\n" +
                    "- Once you disconnect from a device, most endpoints won't work until you reconnect."
                );
        }

        public override async Task Handle(DisconnectDeviceRequest r, CancellationToken ct)
        {
            var device = deviceManager.GetConnectedDevice(r.DeviceId);
            
            if (device is null)
            {
                SendNotFound($"The device {r.DeviceId} was not found.");
                return;
            }
            deviceManager.ClosePort(device.Cart.DeviceId!);
            
            Response = new();
            Send();
        }
    }
}