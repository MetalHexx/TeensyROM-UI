using RadEndpoints;
using System.ComponentModel.DataAnnotations;

namespace TeensyRom.Api.Endpoints.Serial.GetLogs
{
    public class LogDto
    {
        [Required] public string Message { get; set; } = string.Empty;
    }
}