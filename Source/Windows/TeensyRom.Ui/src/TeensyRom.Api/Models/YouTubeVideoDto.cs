using System.ComponentModel.DataAnnotations;

namespace TeensyRom.Api.Models
{
    /// <summary>
    /// Data transfer object representing a YouTube video associated with a file.
    /// </summary>
    public class YouTubeVideoDto
    {
        /// <summary>
        /// The YouTube video ID.
        /// </summary>
        [Required] public string VideoId { get; set; } = string.Empty;

        /// <summary>
        /// The full URL to the YouTube video.
        /// </summary>
        [Required] public string Url { get; set; } = string.Empty;

        /// <summary>
        /// The YouTube channel name.
        /// </summary>
        [Required] public string Channel { get; set; } = string.Empty;

        /// <summary>
        /// The subtune number associated with this video.
        /// </summary>
        [Required] public int Subtune { get; set; }
    }
}