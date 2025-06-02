using TeensyRom.Api.Models;
using TeensyRom.Core.Abstractions;

namespace TeensyRom.Api.Endpoints.ConnectDevice
{
    public class ConnectDeviceEndpoint (IDeviceConnectionManager deviceManager) : RadEndpoint<ConnectDeviceRequest, ConnectDeviceResponse>
    {
        public override void Configure()
        {
            Post("/devices/{deviceId}/connect")
                .Produces<ConnectDeviceResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithName("ConnectDevice")
                .WithSummary("Connect Device")
                .WithTags("Devices")
                .WithDescription(
                    "Connects to a TeensyROM device.\n\n" +
                    "- You must be connected to a TeensyROM device in order to call most other endpoints."
                );
        }

        public override async Task Handle(ConnectDeviceRequest r, CancellationToken _)
        {
            var device = deviceManager.Connect(r.DeviceId);

            if (device is not null) 
            {
                Response = new()
                {
                    ConnectedCart = CartDto.FromDevice(device)
                };
                Send();
                return;
            }
            SendNotFound("Could not connect.  I'll keep trying to re-connect.  Make sure your Commodore is turned on and connected via serial.");
        }
    }
}