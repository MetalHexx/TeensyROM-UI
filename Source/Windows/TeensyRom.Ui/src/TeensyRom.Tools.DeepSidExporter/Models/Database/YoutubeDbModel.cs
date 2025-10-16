namespace TeensyRom.Tools.DeepSidExporter.Models.Database;

/// <summary>
/// Represents a raw record from the youtube table
/// </summary>
public class YoutubeDbModel
{
    public int FileId { get; set; }                // mediumint unsigned
    public string VideoId { get; set; } = string.Empty;      // varchar(32)
    public int Subtune { get; set; }               // smallint unsigned
    public string Channel { get; set; } = string.Empty;      // varchar(128)
}