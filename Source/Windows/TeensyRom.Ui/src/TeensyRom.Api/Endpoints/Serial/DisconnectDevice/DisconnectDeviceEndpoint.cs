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
                .WithDocument(tag: "Devices", desc: "Close device port.");
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