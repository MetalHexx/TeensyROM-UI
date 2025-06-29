using System.ComponentModel.DataAnnotations;

namespace TeensyRom.Api.Endpoints.Serial.PingDevice
{
    public class PingDeviceRequest
    {
        /// <summary>
        /// The unique ID of the TeensyROM device.
        /// </summary>
        [FromRoute]
        public string DeviceId { get; set; } = string.Empty;
    }
    public class PingDeviceResponse
    {
        /// <summary>
        /// A message indicating the result of the operation.
        /// </summary>
        [Required] public string Message { get; set; } = "Success!";
    }
}
