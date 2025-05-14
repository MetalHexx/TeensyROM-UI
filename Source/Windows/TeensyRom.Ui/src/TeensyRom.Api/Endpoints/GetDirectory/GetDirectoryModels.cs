using RadEndpoints;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Storage;

namespace TeensyRom.Api.Endpoints.GetDirectory
{
    public class GetDirectoryRequest 
    {
        [FromRoute]
        public string? DeviceId { get; set; } = string.Empty;
        [FromQuery]
        public string? Path { get; set; } = string.Empty;
        [FromRoute]
        public TeensyStorageType StorageType { get; set; } = TeensyStorageType.SD;
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

    public class GetDirectoryResponse
    {
        public IStorageCacheItem StorageItem { get; set; } = null!;
        public string Message { get; set; } = "Success!";
    }
}