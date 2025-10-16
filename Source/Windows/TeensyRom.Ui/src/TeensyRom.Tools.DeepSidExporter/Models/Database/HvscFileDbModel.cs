namespace TeensyRom.Tools.DeepSidExporter.Models.Database;

/// <summary>
/// Represents a raw record from the hvsc_files table
/// </summary>
public class HvscFileDbModel
{
    public int Id { get; set; }                    // mediumint unsigned
    public string Fullname { get; set; } = string.Empty;     // varchar(128)
    public string Name { get; set; } = string.Empty;         // varchar(128)
    public string Author { get; set; } = string.Empty;       // varchar(128)
    public string Copyright { get; set; } = string.Empty;    // varchar(128)
    public string Player { get; set; } = string.Empty;       // varchar(48)
    public string Lengths { get; set; } = string.Empty;      // varchar(2048)
    public string Type { get; set; } = string.Empty;         // varchar(16)
    public string Version { get; set; } = string.Empty;      // varchar(8) - NOT int!
    public string PlayerType { get; set; } = string.Empty;   // varchar(32)
    public string PlayerCompat { get; set; } = string.Empty; // varchar(32)
    public string ClockSpeed { get; set; } = string.Empty;   // varchar(16)
    public string SidModel { get; set; } = string.Empty;     // varchar(32)
    public int DataOffset { get; set; }            // smallint unsigned
    public int DataSize { get; set; }              // smallint unsigned
    public int LoadAddr { get; set; }              // smallint unsigned
    public int InitAddr { get; set; }              // smallint unsigned
    public int PlayAddr { get; set; }              // smallint unsigned
    public int Subtunes { get; set; }              // smallint unsigned
    public int StartSubtune { get; set; }          // smallint unsigned
    public string Hash { get; set; } = string.Empty;         // varchar(32)
    public string Stil { get; set; } = string.Empty;         // varchar(8192) - NOT nullable in DB
    public int New { get; set; }                   // smallint unsigned
    public int Updated { get; set; }               // smallint unsigned
    public string CsdbType { get; set; } = string.Empty;     // varchar(32)
    public int CsdbId { get; set; }                // mediumint unsigned - NOT nullable
    public string Application { get; set; } = string.Empty;  // enum
    public string Gb64 { get; set; } = string.Empty;         // varchar(10240)
}