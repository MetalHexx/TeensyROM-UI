using TeensyRom.Api.Models;
using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.ValueObjects;

namespace TeensyRom.Api.Endpoints.Files.FavoriteFile
{
    public class FavoriteFileEndpoint(IDeviceConnectionManager deviceManager) : RadEndpoint<SaveFavoriteRequest, SaveFavoriteResponse>
    {
        public override void Configure()
        {
            Post("/devices/{deviceId}/storage/{storageType}/favorite")
                .Produces<SaveFavoriteResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status502BadGateway)
                .WithName("SaveFavorite")
                .WithSummary("Save Favorite")
                .WithDescription("Saves a file as a favorite, creating a copy in the appropriate favorites directory.")
                .WithTags("Files");
        }

        public override async Task Handle(SaveFavoriteRequest r, CancellationToken ct)
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

            if (file is not LaunchableItem launchItem)
            {
                SendValidationError($"The file {r.FilePath} is not a valid file type for favoriting.");
                return;
            }

            var savedFav = await storage.SaveFavorite(launchItem, r.StorageType, ct);

            if (savedFav is null)
            {
                SendExternalError("Failed to save favorite. Please try again.");
                return;
            }

            var favPath = StorageHelper.GetFavoritePath(launchItem.FileType);
            var favoriteFileDto = FileItemDto.FromLaunchable(savedFav);
            favoriteFileDto.IsFavorite = true;

            Response = new()
            {
                Message = "Success!",
                FavoriteFile = favoriteFileDto,
                FavoritePath = favPath.Value
            };
            Send();
        }
    }
}
