using RadEndpoints;
using TeensyRom.Api.Endpoints.Common;

namespace TeensyRom.Api.Endpoints.OpenPort
{
    public class OpenPortRequest { }

    public class OpenPortResponse : ApiResponse
    {
        public string ComPort { get; set; } = string.Empty;
        public override string Message { get; set; } = "Attempting to connect to first available TeensyRom cartridge";
    }
}