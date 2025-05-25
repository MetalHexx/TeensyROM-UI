using Microsoft.AspNetCore.Mvc.Rendering;
using RadEndpoints;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Settings;

namespace TeensyRom.Api.Endpoints.Files.LaunchRandom
{
    public class LaunchRandomRequest 
    {
        [FromRoute] public string DeviceId { get; set; } = string.Empty;
        [FromRoute] public TeensyStorageType StorageType { get; set; }
        [FromQuery] public TeensyFilterType? FilterType { get; set; } = TeensyFilterType.All;
        [FromQuery] public StorageScope? Scope { get; set; } = StorageScope.DirDeep;
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

    public class LaunchRandomResponse
    {
        public FileItemDto LaunchedFile { get; set; } = null!;
        public string Message { get; set; } = "Success!";
    }
}