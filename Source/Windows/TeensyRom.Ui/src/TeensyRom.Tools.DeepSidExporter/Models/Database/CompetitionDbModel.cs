namespace TeensyRom.Tools.DeepSidExporter.Models.Database;

/// <summary>
/// Represents a raw record from the competitions_cache table
/// </summary>
public class CompetitionDbModel
{
    public int FileId { get; set; }                // mediumint unsigned
    public string Name { get; set; } = string.Empty;         // varchar(64)
    public int Place { get; set; }                 // smallint (can be negative!)
}