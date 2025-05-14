using RadEndpoints;
using TeensyRom.Core.Abstractions;

namespace TeensyRom.Api.Endpoints.GetDirectory
{
    public class GetDirectoryEndpoint(IDeviceConnectionManager deviceManager) : RadEndpoint<GetDirectoryRequest, GetDirectoryResponse>
    {
        public override void Configure()
        {
            Get("/devices/{deviceId}/storage/{storageType}/directories")
                .Produces<GetDirectoryResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithDocument(tag: "Storage", desc: "Gets a directory for given storage device");
        }

        public override async Task Handle(GetDirectoryRequest r, CancellationToken ct)
        {
            var device = deviceManager.GetConnectedDevice(r.DeviceId!);
            if (device is null)
            {
                SendNotFound($"The device {r.DeviceId} was not found.");
                return;
            }
            var storage = device.GetStorage(r.StorageType);
            if (storage is null)
            {
                SendNotFound($"The storage {r.StorageType} is not available.");
                return;
            }
            var storageItem = await storage.GetDirectory(r.Path!);
            if (storageItem is null)
            {
                SendNotFound($"The directory {r.Path} was not found.");
                return;
            }
            Response = new()
            {
                StorageItem = storageItem
            };
            Send();
        }
    }
}