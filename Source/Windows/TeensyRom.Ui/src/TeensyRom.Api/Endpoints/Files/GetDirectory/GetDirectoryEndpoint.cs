using RadEndpoints;
using TeensyRom.Api.Models;
using TeensyRom.Core.Abstractions;

namespace TeensyRom.Api.Endpoints.Files.GetDirectory
{
    public class GetDirectoryEndpoint(IDeviceConnectionManager deviceManager) : RadEndpoint<GetDirectoryRequest, GetDirectoryResponse>
    {
        public override void Configure()
        {
            Get("/devices/{deviceId}/storage/{storageType}/directories")
                .Produces<GetDirectoryResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithName("GetDirectory")
                .WithSummary("Get Directory")
                .WithTags("Files")
                .WithDescription(
                    "Gets a directory for given storage device.\n\n" +
                    "- Returns metadata for all files in the directory.\n" +
                    "- This is not recursive and will only include the files for the requested directory.\n" +
                    "- Make another request to get subdirectory content."
                );
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
                StorageItem = StorageCacheDto.FromCache(storageItem)
            };
            Send();
        }
    }
}