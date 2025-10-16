using System.ComponentModel.DataAnnotations;

namespace TeensyRom.Api.Models
{
    /// <summary>
    /// Data transfer object representing a competition entry associated with a file.
    /// </summary>
    public class CompetitionDto
    {
        /// <summary>
        /// The name of the competition.
        /// </summary>
        [Required] public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The place/position achieved in the competition (e.g., 1st, 2nd, 3rd).
        /// </summary>
        public int? Place { get; set; }
    }
}