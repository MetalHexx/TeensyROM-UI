using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using RadEndpoints;
using System.Text.RegularExpressions;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Api.Endpoints.Files.LaunchFile
{
    public class LaunchFileRequest
    {
        [FromRoute]
        public string DeviceId { get; set; } = string.Empty;
        [FromQuery]
        public string Path { get; set; } = string.Empty;
        [FromQuery]
        public TeensyStorageType StorageType { get; set; } = TeensyStorageType.SD;
    }

    public class LaunchFileRequestValidator : AbstractValidator<LaunchFileRequest>
    {
        public LaunchFileRequestValidator()
        {
            RuleFor(x => x.DeviceId)
                .NotEmpty().WithMessage("Device ID is required.")
                .Must(deviceId => deviceId.IsValidFilenameSafeHash()).WithMessage("Device ID must be a valid filename-safe hash of 8 characters long.");

            RuleFor(x => x.Path)
            .NotEmpty().WithMessage("Path is required.")
            .Must(path => path.IsValidUnixFilePath()).WithMessage("Path must be a valid Unix-style file path.");

            RuleFor(x => x.StorageType)
                .IsInEnum().WithMessage("Storage type must be a valid enum value.");
        }
    }

    public class LaunchFileResponse
    {
        public string Message { get; set; } = "Success!";
    }
}