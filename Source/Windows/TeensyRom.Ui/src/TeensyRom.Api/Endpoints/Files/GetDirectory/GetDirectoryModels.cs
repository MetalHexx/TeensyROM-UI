using CsvHelper.Configuration.Attributes;
using RadEndpoints;
using System.ComponentModel.DataAnnotations;
using TeensyRom.Api.Models;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Api.Endpoints.Files.GetDirectory
{
    /// <summary>
    /// Request model for retrieving the contents of a directory from a TeensyROM device's storage.
    /// </summary>
    public class GetDirectoryRequest
    {
        /// <summary>
        /// The unique ID of the TeensyROM device.
        /// </summary>
        [FromRoute] public string DeviceId { get; set; } = string.Empty;

        /// <summary>
        /// The storage type to query (SD or USB).
        /// </summary>
        [FromRoute] public TeensyStorageType StorageType { get; set; } = TeensyStorageType.SD;

        /// <summary>
        /// The path to the directory to retrieve. Must be a valid Unix-style file path.
        /// </summary>
        [FromQuery] public string? Path { get; set; } = string.Empty;
    }

    public class GetDirectoryRequestValidator : AbstractValidator<GetDirectoryRequest>
    {
        public GetDirectoryRequestValidator()
        {
            RuleFor(x => x.DeviceId)
                .NotEmpty().WithMessage("Device ID is required.")
                .Must(deviceId => deviceId.IsValidFilenameSafeHash()).WithMessage("Device ID must be a valid filename-safe hash of 8 characters long.");

            RuleFor(x => x.Path)
                .NotEmpty().WithMessage("Path is required.")
                .Must(path => path!.IsValidUnixFilePath()).WithMessage("Path must be a valid Unix-style file path.");

            RuleFor(x => x.StorageType)
                .IsInEnum().WithMessage("Storage type must be a valid enum value.");
        }
    }

    /// <summary>
    /// Response model containing the directory metadata and its contents.
    /// </summary>
    public class GetDirectoryResponse
    {
        /// <summary>
        /// The directory and its immediate files and subdirectories.
        /// </summary>
        [Required] public StorageCacheDto StorageItem { get; set; } = null!;

        /// <summary>
        /// A message indicating the result of the operation.
        /// </summary>
        [Required] public string Message { get; set; } = "Success!";
    }
}