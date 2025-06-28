using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
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
                .WithName("IndexAll")
                .WithSummary("Index All")
                .WithTags("Files")
                .WithDescription(
                    "Indexes all storage for all connected TeensyROM devices.\n\n" +
                    "- This will recursively index all storage devices.\n" +
                    "- Multiple devices will be indexed in parallel, one device type at a time.\n" +
                    "- This could take a few minutes if you have a lot of data.\n" +
                    "- Don't touch your commodores while indexing is in progress."
                );
        }

        public override async Task Handle(CancellationToken ct)
        {
            var devices = deviceManager.GetConnectedDevices();

            if (devices.Count == 0)
            {
                SendNotFound("No devices found.");
                return;
            }

            var sdTasks = devices
                .Where(d => d.Cart.SdStorage.Available)
                .Select(d => new
                {
                    DeviceId = d.DeviceId,
                    StorageType = "SD",
                    Task = d.SdStorage.CacheAll(ct)
                })
                .ToList();            

            await Task.WhenAll(sdTasks.Select(t => t.Task));

            var usbTasks = devices
                .Where(d => d.Cart.UsbStorage.Available)
                .Select(d => new
                {
                    d.DeviceId,
                    StorageType = "USB",
                    Task = d.UsbStorage.CacheAll(ct)
                })
                .ToList();

            await Task.WhenAll(usbTasks.Select(t => t.Task));

            var failedItems = sdTasks
                .Where(t => !t.Task.Result)
                .Concat(usbTasks.Where(t => !t.Task.Result))
                .Select(t => new
                {
                    t.DeviceId,
                    t.StorageType
                })
                .ToList();

            if (failedItems.Count > 0)
            {
                var extensions = failedItems
                    .GroupBy(f => f.DeviceId!)
                    .ToDictionary(
                        g => g.Key,
                        g => (object)g.Select(x => x.StorageType).ToList()
                    );

                SendProblem(TypedResults.Problem(
                    title: "Indexing failure.",
                    statusCode: StatusCodes.Status500InternalServerError,
                    detail: "Some storage devices had issues indexing.  See error list.",
                    extensions: extensions!
                ));
                return;
            }

            Response = new();
            Send();
        }
    }
}
