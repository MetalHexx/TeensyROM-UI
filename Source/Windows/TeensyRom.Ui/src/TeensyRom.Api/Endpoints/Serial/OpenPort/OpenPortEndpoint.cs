using TeensyRom.Core.Abstractions;

namespace TeensyRom.Api.Endpoints.OpenPort
{
    public class OpenPortEndpoint (IDeviceConnectionManager deviceManager) : RadEndpoint<OpenPortRequest, OpenPortResponse>
    {
        public override void Configure()
        {
            Get("/serial/carts/{deviceId}/open")
                .Produces<OpenPortResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithDocument(tag: "Serial", desc: "Connects to a TeensyRom device given an ID.");
        }

        public override async Task Handle(OpenPortRequest r, CancellationToken _)
        {
            var device = await deviceManager.Connect(r.DeviceId);

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