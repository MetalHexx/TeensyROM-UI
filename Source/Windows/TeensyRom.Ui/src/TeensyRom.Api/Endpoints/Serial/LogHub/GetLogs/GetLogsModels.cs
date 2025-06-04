using RadEndpoints;
using System.ComponentModel.DataAnnotations;

namespace TeensyRom.Api.Endpoints.GetLogs
{
    public class LogDto
    {
        [Required] public string Message { get; set; } = string.Empty;
    }
}