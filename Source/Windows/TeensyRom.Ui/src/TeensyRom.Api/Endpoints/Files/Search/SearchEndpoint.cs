using TeensyRom.Api.Models;
using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Settings;

namespace TeensyRom.Api.Endpoints.Files.Search
{
    public class SearchEndpoint(IDeviceConnectionManager deviceManager) : RadEndpoint<SearchRequest, SearchResponse>
    {
        public override void Configure()
        {
            Get("/devices/{deviceId}/storage/{storageType}/search")
                .Produces<SearchResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .WithName("Search")
                .WithSummary("Search Files")
                .WithTags("Files")
                .WithDescription(
                    "Searches for files in the specified storage device based on search text and filter criteria.\n\n" +
                    "- Searches through file names, titles, creators, and descriptions.\n" +
                    "- Returns metadata for all matching files.\n" +
                    "- Supports file type filtering (All, Games, Music, Images, Hex).\n" +
                    "- Excludes favorites and playlist directories from search results.\n" +
                    "- Uses weighted search algorithm to rank results by relevance."
                );
        }

        public override Task Handle(SearchRequest r, CancellationToken ct)
        {
            var device = deviceManager.GetConnectedDevice(r.DeviceId!);
            if (device is null)
            {
                SendNotFound($"The device {r.DeviceId} was not found.");
                return Task.CompletedTask;
            }
            
            var storage = device.GetStorage(r.StorageType);
            if (storage is null)
            {
                SendNotFound($"The storage {r.StorageType} is not available.");
                return Task.CompletedTask;
            }

            var filterType = r.FilterType ?? TeensyFilterType.All;
            var searchResults = storage.Search(r.SearchText, filterType);
            
            var fileItems = searchResults
                .Select(FileItemDto.FromLaunchable)
                .ToList();

            Response = new()
            {
                Files = fileItems,
                SearchText = r.SearchText,
                TotalCount = fileItems.Count,
                Message = fileItems.Count > 0 
                    ? $"Found {fileItems.Count} file(s) matching '{r.SearchText}'" 
                    : $"No files found matching '{r.SearchText}'"
            };
            Send();
            return Task.CompletedTask;
        }
    }
}