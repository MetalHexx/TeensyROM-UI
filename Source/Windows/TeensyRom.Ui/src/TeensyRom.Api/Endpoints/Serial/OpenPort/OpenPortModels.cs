using Microsoft.AspNetCore.Mvc;
using RadEndpoints;
using TeensyRom.Api.Endpoints.Common;
using TeensyRom.Core.Serial;

namespace TeensyRom.Api.Endpoints.OpenPort
{
    public class OpenPortRequest 
    {
        [FromRoute]
        public string DeviceId { get; set; } = string.Empty;
    }

    public class OpenPortRequestValidator : AbstractValidator<OpenPortRequest>
    {
        public OpenPortRequestValidator()
        {
            RuleFor(x => x.DeviceId)
                .NotEmpty()
                .WithMessage("DeviceId cannot be empty.");
        }
    }

    public class OpenPortResponse : ApiResponse
    {
        public Cart ConnectedCart { get; set; } = null!;
        public override string Message { get; set; } = "Connection successful!";
    }
}