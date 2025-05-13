using RadEndpoints;
using TeensyRom.Core.Abstractions;

namespace TeensyRom.Api.Endpoints.ClosePort
{
    public class ClosePortEndpoint(IDeviceConnectionManager deviceManager) : RadEndpoint<ClosePortRequest, ClosePortResponse>
    {
        public override void Configure()
        {
            Delete("/serials/{deviceId}")
                .Produces<ClosePortResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithDocument(tag: "Serial", desc: "Close device port.");
        }

        public override async Task Handle(ClosePortRequest r, CancellationToken ct)
        {
            var device = deviceManager.GetConnectedDevice(r.DeviceId);
            
            if (device is null)
            {
                SendNotFound($"The device {r.DeviceId} was not found.");
                return;
            }
            
            Response = new();
            Send();
        }
    }
}