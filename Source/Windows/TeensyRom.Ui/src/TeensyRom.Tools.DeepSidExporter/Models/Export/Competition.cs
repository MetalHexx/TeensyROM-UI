using System.Text.Json.Serialization;

namespace TeensyRom.Tools.DeepSidExporter.Models.Export;

/// <summary>
/// Represents a competition entry associated with a file
/// </summary>
public class Competition
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("place")]
    public int? Place { get; set; }
}