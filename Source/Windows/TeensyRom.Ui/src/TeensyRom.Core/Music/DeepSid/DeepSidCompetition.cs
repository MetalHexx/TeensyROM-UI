using System.Text.Json.Serialization;

namespace TeensyRom.Core.Music.DeepSid;

/// <summary>
/// Represents a competition entry
/// </summary>
public class DeepSidCompetition
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("place")]
    public int? Place { get; set; }
}
