namespace TeensyRom.Tools.DeepSidExporter.Models.Database;

/// <summary>
/// Represents a raw record from the composers table
/// </summary>
public class ComposerDbModel
{
    public int Id { get; set; }                    // mediumint unsigned
    public string Fullname { get; set; } = string.Empty;     // varchar(128)
    public string Name { get; set; } = string.Empty;         // varchar(48)
    public string Shortname { get; set; } = string.Empty;    // varchar(32)
    public string Handles { get; set; } = string.Empty;      // varchar(128)
    public string Shorthandle { get; set; } = string.Empty;  // varchar(48)
    public int Active { get; set; }                // year - this is the issue! It's an int
    public string Born { get; set; } = string.Empty;         // date
    public string Died { get; set; } = string.Empty;         // date
    public string Country { get; set; } = string.Empty;      // varchar(128)
    public string CsdbType { get; set; } = string.Empty;     // varchar(32)
    public int CsdbId { get; set; }                // mediumint unsigned
    public string ImageSource { get; set; } = string.Empty;  // enum
    public string Notable { get; set; } = string.Empty;      // varchar(512)
    public string Employment { get; set; } = string.Empty;   // varchar(512)
    public string Affiliation { get; set; } = string.Empty;  // varchar(128)
    public string Brand { get; set; } = string.Empty;        // varchar(64)
    public string BrandDark { get; set; } = string.Empty;    // varchar(64)
}