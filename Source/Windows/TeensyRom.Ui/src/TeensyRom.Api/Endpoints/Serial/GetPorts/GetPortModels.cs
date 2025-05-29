using System.ComponentModel.DataAnnotations;
using TeensyRom.Api.Models;

namespace TeensyRom.Api.Endpoints.Serial.GetPorts
{
    public class GetPortsRequest();
    public class GetPortsResponse : ApiResponse
    {
        [Required] public string[] Ports { get; set; } = [];
    };
}
