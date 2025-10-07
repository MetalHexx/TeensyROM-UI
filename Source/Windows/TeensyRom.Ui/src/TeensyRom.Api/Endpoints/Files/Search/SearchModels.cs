using RadEndpoints;
using System.ComponentModel.DataAnnotations;
using TeensyRom.Api.Models;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Settings;

namespace TeensyRom.Api.Endpoints.Files.Search
{
    /// <summary>
    /// Request model for searching files in a TeensyROM device's storage.
    /// </summary>
    public class SearchRequest
    {
        /// <summary>
        /// The unique ID of the TeensyROM device.
        /// </summary>
        [FromRoute] public string DeviceId { get; set; } = string.Empty;

        /// <summary>
        /// The storage type to search (SD or USB).
        /// </summary>
        [FromRoute] public TeensyStorageType StorageType { get; set; } = TeensyStorageType.SD;

        /// <summary>
        /// The search text to look for in file names, titles, creators, and descriptions.
        /// </summary>
        [FromQuery] public string SearchText { get; set; } = string.Empty;

        /// <summary>
        /// The filter type to apply when searching files (e.g., All, Games, Music).
        /// </summary>
        [FromQuery] public TeensyFilterType? FilterType { get; set; } = TeensyFilterType.All;

        /// <summary>
        /// The number of results to skip for pagination (default: 0).
        /// </summary>
        [FromQuery] public int Skip { get; set; } = 0;

        /// <summary>
        /// The maximum number of results to return (default: 50, max: 200).
        /// </summary>
        [FromQuery] public int Take { get; set; } = 50;
    }

    public class SearchRequestValidator : AbstractValidator<SearchRequest>
    {
        public SearchRequestValidator()
        {
            RuleFor(x => x.DeviceId)
                .NotEmpty().WithMessage("Device ID is required.")
                .Must(deviceId => deviceId.IsValidFilenameSafeHash()).WithMessage("Device ID must be a valid filename-safe hash of 8 characters long.");

            RuleFor(x => x.SearchText)
                .NotEmpty().WithMessage("Search text is required.")
                .MinimumLength(1).WithMessage("Search text must be at least 2 characters long.")
                .MaximumLength(100).WithMessage("Search text must be no more than 100 characters long.");

            RuleFor(x => x.StorageType)
                .IsInEnum().WithMessage("Storage type must be a valid enum value.");

            RuleFor(x => x.FilterType)
                .IsInEnum().WithMessage("Filter type must be a valid enum value.");

            RuleFor(x => x.Skip)
                .GreaterThanOrEqualTo(0).WithMessage("Skip must be greater than or equal to 0.");

            RuleFor(x => x.Take)
                .GreaterThan(0).WithMessage("Take must be greater than 0.")
                .LessThanOrEqualTo(1000).WithMessage("Take must be no more than 200.");
        }
    }

    /// <summary>
    /// Response model containing the search results.
    /// </summary>
    public class SearchResponse
    {
        /// <summary>
        /// The list of files that match the search criteria.
        /// </summary>
        [Required] public List<FileItemDto> Files { get; set; } = [];

        /// <summary>
        /// The search text that was used.
        /// </summary>
        [Required] public string SearchText { get; set; } = string.Empty;

        /// <summary>
        /// The total number of files found (before pagination).
        /// </summary>
        [Required] public int TotalCount { get; set; } = 0;

        /// <summary>
        /// The number of results returned in this page.
        /// </summary>
        [Required] public int Count { get; set; } = 0;

        /// <summary>
        /// The number of results that were skipped for pagination.
        /// </summary>
        [Required] public int Skip { get; set; } = 0;

        /// <summary>
        /// The maximum number of results that were requested.
        /// </summary>
        [Required] public int Take { get; set; } = 0;

        /// <summary>
        /// Indicates whether there are more results available.
        /// </summary>
        [Required] public bool HasMore { get; set; } = false;

        /// <summary>
        /// A message indicating the result of the operation.
        /// </summary>
        [Required] public string Message { get; set; } = "Success!";
    }
}