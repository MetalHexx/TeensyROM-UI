using RadEndpoints;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Api.Endpoints.Files.Index
{
    public class IndexRequest 
    {
        [FromRoute]
        public string DeviceId { get; set; } = null!;
        [FromRoute]
        public TeensyStorageType StorageType { get; set; } = TeensyStorageType.SD;
        [FromBody]
        public string? Path { get; set; }
    }

    public class IndexRequestValidator : AbstractValidator<IndexRequest>
    {
        public IndexRequestValidator()
        {
            RuleFor(x => x.DeviceId)
                .Must(deviceId => deviceId.IsValidFilenameSafeHash())
                .WithMessage("Invalid Device Id.");

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