using System.ComponentModel.DataAnnotations;

namespace TeensyRom.Api.Models
{
    public class ApiResponse
    {
        [Required] public virtual string Message { get; set; } = "Success";
    }
}
