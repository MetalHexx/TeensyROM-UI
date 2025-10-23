using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using RadEndpoints;
using System.ComponentModel.DataAnnotations;
using TeensyRom.Api.Models;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Api.Endpoints.Files.FavoriteFile
{
    /// <summary>
    /// Request model for saving a file as a favorite in TeensyROM storage.
    /// </summary>
    public class SaveFavoriteRequest
    {
        /// <summary>
        /// The unique ID of the TeensyROM device.
        /// </summary>
        [FromRoute]
        public string DeviceId { get; set; } = string.Empty;

        /// <summary>
        /// The storage type where the file is located (SD or USB).
        /// </summary>
        [FromRoute]
        public TeensyStorageType StorageType { get; set; } = TeensyStorageType.SD;

        /// <summary>
        /// The path to the file to save as favorite. Must be a valid Unix-style file path.
        /// </summary>
        [FromQuery]
        public string FilePath { get; set; } = string.Empty;
    }

    public class SaveFavoriteRequestValidator : AbstractValidator<SaveFavoriteRequest>
    {
        public SaveFavoriteRequestValidator()
        {
            RuleFor(x => x.DeviceId)
                .NotEmpty().WithMessage("Device ID is required.")
                .Must(deviceId => deviceId.IsValidFilenameSafeHash()).WithMessage("Device ID must be a valid filename-safe hash of 8 characters long.");

            RuleFor(x => x.FilePath)
                .NotEmpty().WithMessage("Path is required.")
                .Must(path => path.IsValidUnixFilePath()).WithMessage("Path must be a valid Unix-style file path.");

            RuleFor(x => x.StorageType)
                .IsInEnum().WithMessage("Storage type must be a valid enum value.");
        }
    }

    /// <summary>
    /// Response model for the result of a save favorite operation.
    /// </summary>
    public class SaveFavoriteResponse
    {
        /// <summary>
        /// A message indicating the result of the operation.
        /// </summary>
        [Required] public string Message { get; set; } = "Success!";

        /// <summary>
        /// The newly created favorite copy of the file.
        /// </summary>
        [Required] public FileItemDto FavoriteFile { get; set; } = null!;

        /// <summary>
        /// The directory path where the favorite file was saved.
        /// </summary>
        [Required] public string FavoritePath { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for removing a file from favorites in TeensyROM storage.
    /// </summary>
    public class RemoveFavoriteRequest
    {
        /// <summary>
        /// The unique ID of the TeensyROM device.
        /// </summary>
        [FromRoute]
        public string DeviceId { get; set; } = string.Empty;

        /// <summary>
        /// The storage type where the file is located (SD or USB).
        /// </summary>
        [FromRoute]
        public TeensyStorageType StorageType { get; set; } = TeensyStorageType.SD;

        /// <summary>
        /// The path to the file to remove from favorites. Can be either the original or favorite file path.
        /// </summary>
        [FromQuery]
        public string FilePath { get; set; } = string.Empty;
    }

    public class RemoveFavoriteRequestValidator : AbstractValidator<RemoveFavoriteRequest>
    {
        public RemoveFavoriteRequestValidator()
        {
            RuleFor(x => x.DeviceId)
                .NotEmpty().WithMessage("Device ID is required.")
                .Must(deviceId => deviceId.IsValidFilenameSafeHash()).WithMessage("Device ID must be a valid filename-safe hash of 8 characters long.");

            RuleFor(x => x.FilePath)
                .NotEmpty().WithMessage("Path is required.")
                .Must(path => path.IsValidUnixFilePath()).WithMessage("Path must be a valid Unix-style file path.");

            RuleFor(x => x.StorageType)
                .IsInEnum().WithMessage("Storage type must be a valid enum value.");
        }
    }

    /// <summary>
    /// Response model for the result of a remove favorite operation.
    /// </summary>
    public class RemoveFavoriteResponse
    {
        /// <summary>
        /// A message indicating the result of the operation.
        /// </summary>
        [Required] public string Message { get; set; } = "Success!";
    }
}
