using MediatR;
using Microsoft.OpenApi.Writers;
using TeensyRom.Api.Models;
using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Commands.File.LaunchFile;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Settings;
using TeensyRom.Core.ValueObjects;

namespace TeensyRom.Api.Endpoints.Player.LaunchRandom
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
                .WithName("LaunchRandom")
                .WithSummary("Launch Random File")
                .WithTags("Player")
                .WithDescription(
                    "Launches a random file given a device, storage, filter and starting directory location.\n\n" +
                    "- Starting Directory: Starting directory to look for a random file.\n" + 
                    "- Scope: `Storage` - Selects a random file anywhere on the specified storage device.\n" +
                    "- Scope: `DirDeep` - Selects a random file from the starting directory or any of its subdirectories.\n" +
                    "- Scope: `DirShallow` - Selects a random file from the starting directory only (subdirectories are not included).\n" +
                    "- Filter: `All` - Any file type will be randomly selected.\n" +
                    "- Filter: `Games` - Only game-related files will be selected (e.g., .prg, .crt, .d64, etc). Includes demos and non-games.\n" +
                    "- Filter: `Music` - Only music or song files will be selected (e.g., .sid, .mus, .mp3, etc).\n" +
                    "- Filter: `Images` - Only image files will be selected (e.g., .koa, .png, etc). Also includes text files (may be improved in a future release)"
                );
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

                r.StartingDirectory is null
                    ? new DirectoryPath(StorageHelper.Remote_Path_Root)
                    : new DirectoryPath(r.StartingDirectory), 

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