using System.Text.Json.Serialization;

namespace TeensyRom.Tools.DeepSidExporter.Models.Export;

/// <summary>
/// Represents a composer in the final JSON export
/// </summary>
public class Composer
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("shortname")]
    public string? Shortname { get; set; }

    [JsonPropertyName("handles")]
    public List<string> Handles { get; set; } = new();

    [JsonPropertyName("shorthandle")]
    public string? Shorthandle { get; set; }

    [JsonPropertyName("active_year")]
    public string? ActiveYear { get; set; }

    [JsonPropertyName("born")]
    public string? Born { get; set; }

    [JsonPropertyName("died")]
    public string? Died { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("csdb")]
    public CsdbInfo? Csdb { get; set; }

    [JsonPropertyName("image_source")]
    public string? ImageSource { get; set; }

    [JsonPropertyName("notable")]
    public string? Notable { get; set; }

    [JsonPropertyName("employment")]
    public string? Employment { get; set; }

    [JsonPropertyName("affiliation")]
    public string? Affiliation { get; set; }

    [JsonPropertyName("brand")]
    public string? Brand { get; set; }

    [JsonPropertyName("branddark")]
    public string? BrandDark { get; set; }

    [JsonPropertyName("links")]
    public List<ComposerLink> Links { get; set; } = new();
}