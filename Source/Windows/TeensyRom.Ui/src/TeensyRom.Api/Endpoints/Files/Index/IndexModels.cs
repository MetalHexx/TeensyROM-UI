using RadEndpoints;
using System.ComponentModel.DataAnnotations;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Api.Endpoints.Files.Index
{
    /// <summary>
    /// Request model for indexing the directory structure of a TeensyROM device's storage.
    /// </summary>
    public class IndexRequest 
    {
        /// <summary>
        /// The unique ID of the TeensyROM device.
        /// </summary>
        [FromRoute]
        public string DeviceId { get; set; } = null!;

        /// <summary>
        /// The storage type to index (SD or USB).
        /// </summary>
        [FromRoute]
        public TeensyStorageType StorageType { get; set; } = TeensyStorageType.SD;

        /// <summary>
        /// The path to the directory to start indexing from. If null, the whole storage device will be indexed.
        /// </summary>
        [FromQuery]
        public string? StartingPath { get; set; }
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

            RuleFor(x => x.StartingPath)
                .Must(path => string.IsNullOrWhiteSpace(path) || path.IsSafeUnixDirectoryName())
                .WithMessage("Path must be a valid Unix path.");
        }
    }

    /// <summary>
    /// Response model for the result of an index operation.
    /// </summary>
    public class IndexResponse
    {
        /// <summary>
        /// A message indicating the result of the operation.
        /// </summary>
        [Required] public string Message { get; set; } = "Success!";
    }
}