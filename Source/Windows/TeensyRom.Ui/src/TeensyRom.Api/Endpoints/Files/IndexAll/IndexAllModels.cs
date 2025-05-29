using RadEndpoints;
using System.ComponentModel.DataAnnotations;

namespace TeensyRom.Api.Endpoints.Files.IndexAll
{
    /// <summary>
    /// Request model for indexing all storage on all connected TeensyROM devices.
    /// </summary>
    public class IndexAllRequest { }

    public class IndexAllRequestValidator : AbstractValidator<IndexAllRequest>
    {
        public IndexAllRequestValidator()
        {
            //TODO: Add validation rules here
        }
    }

    /// <summary>
    /// Response model for the result of an index all operation.
    /// </summary>
    public class IndexAllResponse
    {
        /// <summary>
        /// A message indicating the result of the operation.
        /// </summary>
        [Required] public string Message { get; set; } = "Success!";
    }
}