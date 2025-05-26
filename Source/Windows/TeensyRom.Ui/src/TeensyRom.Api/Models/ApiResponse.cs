namespace TeensyRom.Api.Models
{
    public class ApiResponse
    {
        public bool Success { get; set; } = true;
        public virtual string Message { get; set; } = "Success";
    }
}
