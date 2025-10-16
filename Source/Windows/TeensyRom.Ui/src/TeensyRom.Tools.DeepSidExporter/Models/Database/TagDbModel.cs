namespace TeensyRom.Tools.DeepSidExporter.Models.Database;

/// <summary>
/// Represents a combined record from tags_lookup and tags_info tables
/// </summary>
public class TagDbModel
{
    public int FileId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}