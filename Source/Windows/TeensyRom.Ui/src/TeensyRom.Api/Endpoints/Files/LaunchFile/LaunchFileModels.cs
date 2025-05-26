using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using RadEndpoints;
using System.Text.RegularExpressions;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Api.Endpoints.Files.LaunchFile
{
    /// <summary>
    /// Request model for launching a file from a TeensyROM device's storage.
    /// </summary>
    public class LaunchFileRequest
    {
        /// <summary>
        /// The unique ID of the TeensyROM device.
        /// </summary>
        [FromRoute]
        public string DeviceId { get; set; } = string.Empty;

        /// <summary>
        /// The storage type to launch the file from (SD or USB).
        /// </summary>
        [FromRoute]
        public TeensyStorageType StorageType { get; set; } = TeensyStorageType.SD;

        /// <summary>
        /// The path to the file to launch. Must be a valid Unix-style file path.
        /// </summary>
        [FromQuery]
        public string FilePath { get; set; } = string.Empty;
    }

    public class LaunchFileRequestValidator : AbstractValidator<LaunchFileRequest>
    {
        public LaunchFileRequestValidator()
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
    /// Response model for the result of a launch file operation.
    /// </summary>
    public class LaunchFileResponse
    {
        /// <summary>
        /// A message indicating the result of the operation.
        /// </summary>
        public string Message { get; set; } = "Success!";
    }
}