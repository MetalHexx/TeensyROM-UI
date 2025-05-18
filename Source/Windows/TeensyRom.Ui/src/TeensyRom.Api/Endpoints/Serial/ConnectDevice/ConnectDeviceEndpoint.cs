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
                .WithDocument(tag: "Devices", desc: "Connects to a TeensyRom device given an ID.");
        }

        public override async Task Handle(ConnectDeviceRequest r, CancellationToken _)
        {
            var device = deviceManager.Connect(r.DeviceId);

            if (device is not null) 
            {
                Response = new()
                {
                    ConnectedCart = device.Cart
                };
                Send();
                return;
            }
            SendNotFound("Could not connect.  I'll keep trying to re-connect.  Make sure your Commodore is turned on and connected via serial.");
        }
    }
}