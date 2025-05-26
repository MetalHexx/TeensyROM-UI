using System.Runtime.CompilerServices;
using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Storage;

namespace TeensyRom.Api.Endpoints.Files.Index
{
    public class IndexEndpoint(IDeviceConnectionManager deviceManager) : RadEndpoint<IndexRequest, IndexResponse>
    {
        public override void Configure()
        {
            Post("/devices/{deviceId}/storage/{storageType}/index")
                .Produces<IndexResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithSummary("Index")
                .WithTags("Files")
                .WithDescription(
                    "Indexes the directory structure of a given TeensyROM device and storage type.\n\n" +
                    "- Providing a path will index starting at that directory and all subdirectories below it.\n" +
                    "- Providing no path will index the whole storage device.\n" +
                    "- Don't touch your commodore while indexing is in progress."
                );
        }

        public override async Task Handle(IndexRequest r, CancellationToken ct)
        {
            var device = deviceManager.GetConnectedDevice(r.DeviceId);

            if (device is null) 
            {
                SendNotFound("Device not found.");
                return;
            }

            var result = true;

            if (r.StorageType is TeensyStorageType.SD) 
            {
                result = await HandleCaching(r, device.SdStorage);
            }
            else
            {
                result = await HandleCaching(r, device.UsbStorage);
            }
            if(!result)
            {
                SendInternalError("There was an error indexing.");
                return;
            }
            Response = new();
            Send();
        }

        private async Task<bool> HandleCaching(IndexRequest r, IStorageService s) 
        {
            if (!string.IsNullOrWhiteSpace(r.Path))
            {
                return await s.Cache(r.Path);
            }
            else
            {
                return await s.CacheAll();
            }
        }
    }
}