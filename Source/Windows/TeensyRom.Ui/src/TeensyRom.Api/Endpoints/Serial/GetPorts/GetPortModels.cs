using TeensyRom.Api.Models;

namespace TeensyRom.Api.Endpoints.Serial.GetPorts
{
    public class GetPortsRequest();
    public class GetPortsResponse : ApiResponse
    {
        public string[] Ports { get; set; } = [];
    };
}
