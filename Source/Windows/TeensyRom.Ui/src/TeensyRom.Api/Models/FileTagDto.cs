using System.ComponentModel.DataAnnotations;

namespace TeensyRom.Api.Models
{
    /// <summary>
    /// Data transfer object representing a tag associated with a file.
    /// </summary>
    public class FileTagDto
    {
        /// <summary>
        /// The name of the tag.
        /// </summary>
        [Required] public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The type of the tag (e.g., genre, style, etc.).
        /// </summary>
        [Required] public string Type { get; set; } = string.Empty;
    }
}