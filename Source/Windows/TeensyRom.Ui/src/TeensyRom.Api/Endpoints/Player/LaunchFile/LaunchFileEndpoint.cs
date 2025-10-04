using MediatR;
using TeensyRom.Api.Models;
using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Commands.File.LaunchFile;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.ValueObjects;

namespace TeensyRom.Api.Endpoints.Player.LaunchFile
{
    public class LaunchFileEndpoint(IMediator mediator, IDeviceConnectionManager deviceManager) : RadEndpoint<LaunchFileRequest, LaunchFileResponse>
    {
        public override void Configure()
        {
            Post("/devices/{deviceId}/storage/{storageType}/launch")
                .Produces<LaunchFileResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status502BadGateway)
                .WithName("LaunchFile")
                .WithSummary("Launch File")
                .WithDescription("Launches a file given a valid path to a file stored on the TeensyRom.")
                .WithTags("Player");
        }

        public override async Task Handle(LaunchFileRequest r, CancellationToken ct)
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

            var file = await storage.GetFile(new FilePath(r.FilePath));

            if (file is null) 
            {
                SendNotFound($"The file {r.FilePath} was not found.");
                return;
            }

            if(file is not LaunchableItem launchItem)
            {
                SendValidationError($"The file {r.FilePath} is not launchable.");
                return;
            }

            var launchCommand = new LaunchFileCommand(TeensyStorageType.SD, launchItem, r.DeviceId);
            var result = await mediator.Send(launchCommand, ct);

            // Check if this was a successful launch or compatibility issue vs actual system error
            if (result.IsSuccess || result.IsCompatible == false)
            {
                // Either successful launch or compatibility issue - return success with appropriate message and IsCompatible flag
                var fileDto = FileItemDto.FromLaunchable(launchItem);
                fileDto.IsCompatible = result.IsCompatible; // Set compatibility based on launch result

                Response = new()
                {
                    LaunchedFile = fileDto,
                    IsCompatible = result.IsCompatible,
                    Message = result.IsCompatible ? "Success!" : "File launched but is not compatible with TeensyROM hardware."
                };
                Send();
            }
            else
            {
                // Actual system error (Network, NoResponse, Disconnected, etc.)
                SendExternalError(result.Error);
            }
        }
    }
}