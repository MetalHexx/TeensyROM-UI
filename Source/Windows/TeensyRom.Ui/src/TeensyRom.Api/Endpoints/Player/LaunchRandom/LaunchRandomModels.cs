using Microsoft.AspNetCore.Mvc.Rendering;
using RadEndpoints;
using System.ComponentModel.DataAnnotations;
using TeensyRom.Api.Models;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Settings;

namespace TeensyRom.Api.Endpoints.Player.LaunchRandom
{
    /// <summary>
    /// Request model for launching a random file from a TeensyROM device's storage.
    /// </summary>
    public class LaunchRandomRequest 
    {
        /// <summary>
        /// The unique ID of the TeensyROM device.
        /// </summary>
        [FromRoute] public string DeviceId { get; set; } = string.Empty;

        /// <summary>
        /// The storage type to launch the file from (SD or USB).
        /// </summary>
        [FromRoute] public TeensyStorageType StorageType { get; set; }

        /// <summary>
        /// The filter type to apply when selecting a random file (e.g., All, Games, Music).
        /// </summary>
        [FromQuery] public TeensyFilterType? FilterType { get; set; } = TeensyFilterType.All;

        /// <summary>
        /// The scope to use when searching for a random file (e.g., entire storage, directory deep, directory shallow).
        /// </summary>
        [FromQuery] public StorageScope? Scope { get; set; } = StorageScope.DirDeep;

        /// <summary>
        /// The starting directory path for the random file search.
        /// </summary>
        [FromQuery] public string? StartingDirectory { get; set; } = StorageHelper.Remote_Path_Root;
    }

    public class LaunchRandomRequestValidator : AbstractValidator<LaunchRandomRequest>
    {
        public LaunchRandomRequestValidator()
        {
            RuleFor(x => x.DeviceId)
                .NotEmpty().WithMessage("Device ID is required.")
                .Must(deviceId => deviceId.IsValidFilenameSafeHash())
                .WithMessage("Device ID must be a valid filename-safe hash of 8 characters long.");

            RuleFor(x => x.StartingDirectory)
                .Must(path => string.IsNullOrWhiteSpace(path) || path.IsSafeUnixDirectoryName())
                .WithMessage("Path must be a valid Unix-style file path.");

            RuleFor(x => x.StorageType)
                .IsInEnum().WithMessage("Storage type must be a valid enum value.");

            RuleFor(x => x.FilterType)
                .IsInEnum().WithMessage("Filter type must be a valid enum value.");

            RuleFor(x => x.Scope)
                .IsInEnum().WithMessage("Scope type must be a valid enum value.");
        }
    }

    /// <summary>
    /// Response model for the result of a launch random file operation.
    /// </summary>
    public class LaunchRandomResponse
    {
        /// <summary>
        /// The file that was randomly launched.
        /// </summary>
        [Required] public FileItemDto LaunchedFile { get; set; } = null!;

        /// <summary>
        /// A message indicating the result of the operation.
        /// </summary>
        [Required] public string Message { get; set; } = "Success!";
    }
}