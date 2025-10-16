using System.ComponentModel.DataAnnotations;

namespace TeensyRom.Api.Models
{
    /// <summary>
    /// Data transfer object representing a link associated with a file.
    /// </summary>
    public class FileLinkDto
    {
        /// <summary>
        /// The display name of the link.
        /// </summary>
        [Required] public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The URL of the link.
        /// </summary>
        [Required] public string Url { get; set; } = string.Empty;
    }
}