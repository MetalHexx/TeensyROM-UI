using RadEndpoints;

namespace TeensyRom.Api.Endpoints.Files.IndexAll
{
    public class IndexAllRequest { }

    public class IndexAllRequestValidator : AbstractValidator<IndexAllRequest>
    {
        public IndexAllRequestValidator()
        {
            //TODO: Add validation rules here
        }
    }

    public class IndexAllResponse
    {
        public string Message { get; set; } = "Success!";
    }
}