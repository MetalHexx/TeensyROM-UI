using RadEndpoints;
using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Device;
using TeensyRom.Core.Entities.Device;
using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Api.Endpoints.Files.IndexAll
{
    public class IndexAllEndpoint(IDeviceConnectionManager deviceManager) : RadEndpointWithoutRequest<IndexAllResponse>
    {
        public override void Configure()
        {
            Post("/files/index/all")
                .Produces<IndexAllResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .WithDocument(tag: "File", desc: "Indexes all storage for all connected TeensyRom devices.");
        }

        public override async Task Handle(CancellationToken ct)
        {
            var devices = deviceManager.GetAllConnectedDevices();

            if (devices.Count == 0)
            {
                SendNotFound("No devices found.");
                return;
            }
            var sdTasks = devices
                .Where(d => d.Cart.SdStorage.Available)
                .Select(d => d.SdStorage.CacheAll());

            await Task.WhenAll(sdTasks);

            var usbTasks = devices
                .Where(d => d.Cart.UsbStorage.Available)
                .Select(d => d.UsbStorage.CacheAll());

            await Task.WhenAll(usbTasks);

            Response = new();
            Send();
        }
    }
}