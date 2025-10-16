namespace TeensyRom.Tools.DeepSidExporter.Models.Database;

/// <summary>
/// Represents a raw record from the composers_links table
/// </summary>
public class ComposerLinkDbModel
{
    public int ComposersId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}