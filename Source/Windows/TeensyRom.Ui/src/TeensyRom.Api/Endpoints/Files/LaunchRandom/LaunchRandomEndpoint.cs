using MediatR;
using Microsoft.OpenApi.Writers;
using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Commands.File.LaunchFile;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Settings;

namespace TeensyRom.Api.Endpoints.Files.LaunchRandom
{
    public class LaunchRandomEndpoint(IDeviceConnectionManager deviceManager, IMediator mediator) : RadEndpoint<LaunchRandomRequest, LaunchRandomResponse>
    {
        public override void Configure()
        {
            Post("/devices/{deviceId}/storage/{storageType}/random-launch")
                .Produces<LaunchRandomResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status502BadGateway)
                .WithDocument(tag: "Files", desc: "Launches a random file given a device, storage and starting directory location.");
        }

        public override async Task Handle(LaunchRandomRequest r, CancellationToken ct)
        {
            var device = deviceManager.GetConnectedDevice(r.DeviceId);
            
            if (device is null)
            {
                SendNotFound($"The device {r.DeviceId} was not found.");
                return;
            }

            IStorageService storage = null!;

            if (r.StorageType is TeensyStorageType.SD)
            {
                if (!device.Cart.SdStorage.Available)
                {
                    SendNotFound($"The device {r.DeviceId} does not have an SD card.");
                    return;
                }
                storage = device.SdStorage;

            }
            else if (r.StorageType is TeensyStorageType.USB)
            {
                if (!device.Cart.UsbStorage.Available)
                {
                    SendNotFound($"The device {r.DeviceId} does not have USB storage.");
                    return;
                }
                storage = device.UsbStorage;
            }
            var file = storage.GetRandomFile
            (
                r.Scope ?? StorageScope.Storage, 
                r.StartingDirectory ?? StorageHelper.Remote_Path_Root, 
                r.FilterType ?? TeensyFilterType.All
            );

            if (file is null)
            {
                SendNotFound($"No files were found.");
                return;
            }

            var result = await mediator.Send(new LaunchFileCommand(r.StorageType, file, r.DeviceId));

            if (!result.IsSuccess) 
            {
                SendExternalError($"There was an error launching {file.Path}");
                return;
            }
            Response = new() 
            {
                LaunchedFile = FileItemDto.FromLaunchable(file),
            };
            Send();
        }
    }
}