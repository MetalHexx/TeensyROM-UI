using RadEndpoints;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Api.Endpoints.Files.Index
{
    public class IndexRequest 
    {
        public string DeviceId { get; set; } = null!;
        public TeensyStorageType StorageType { get; set; } = TeensyStorageType.SD;
        public string? Path { get; set; } = null;
    }

    public class IndexRequestValidator : AbstractValidator<IndexRequest>
    {
        public IndexRequestValidator()
        {
            RuleFor(x => x.DeviceId)
                .NotEmpty()
                .WithMessage("Device ID is required.")
                .Must(deviceId => deviceId.IsValidFilenameSafeHash())
                .WithMessage("Device ID must be a valid deviceId.  Only a string of 8 numbers and letters are supported.  No other special characters or spaces.");

            RuleFor(x => x.StorageType)
                .IsInEnum()
                .WithMessage("Storage type must be either SD or USB.");

            RuleFor(x => x.Path)
                .Must(path => string.IsNullOrWhiteSpace(path) || path.IsSafeUnixDirectoryName())
                .WithMessage("Path must be a valid Unix path.");

        }
    }

    public class IndexResponse
    {
        public string Message { get; set; } = "Success!";
    }
}