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
                .WithDocument(tag: "Files", desc: "Indexes the directory structure of a given TR device and storage  type.");
        }

        public override async Task Handle(IndexRequest r, CancellationToken ct)
        {
            var device = deviceManager.GetConnectedDevice(r.DeviceId);

            if (device is null) 
            {
                SendNotFound("Device not found.");
                return;
            }

            if (r.StorageType is TeensyStorageType.SD) 
            {
                await HandleCaching(r, device.SdStorage);
            }
            else
            {
                await HandleCaching(r, device.UsbStorage);
            }
            Response = new();
            Send();
        }

        private async Task HandleCaching(IndexRequest r, IStorageService s) 
        {
            if (!string.IsNullOrWhiteSpace(r.Path))
            {
                await s.Cache(r.Path);
            }
            else
            {
                await s.CacheAll();
            }
        }
    }
}