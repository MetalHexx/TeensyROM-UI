using System.Reflection;
using System.Text.Json;
using TeensyRom.Core.Common;

namespace TeensyRom.Core.Music.DeepSid;

/// <summary>
/// Provides access to the DeepSID HVSC database
/// </summary>
public class DeepSidDatabase : IDeepSidDatabase
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    /// <summary>
    /// All DeepSID records indexed by their filepath
    /// </summary>
    private Dictionary<string, DeepSidRecord> Records { get; set; } = [];

    /// <summary>
    /// Creates a new DeepSidDatabase instance and loads data from the specified JSON file
    /// </summary>
    /// <param name="jsonFilePath">Path to the DeepSID JSON file. If null or empty, attempts to find JSON in default location.</param>
    public DeepSidDatabase(string? jsonFilePath = null)
    {
        var filePath = jsonFilePath ?? GetDeepSidFilePath();

        if (string.IsNullOrWhiteSpace(filePath))
        {
            Records = new Dictionary<string, DeepSidRecord>();
            return;
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"DeepSID JSON file not found: {filePath}");
        }

        var files = ReadDeepSidJson(filePath);
        Records = IndexByFilePath(files);
    }

    /// <summary>
    /// Searches for a DeepSID record by its HVSC filepath
    /// </summary>
    /// <param name="hvscPath">The HVSC filepath for the DeepSID record</param>
    /// <returns>DeepSID record if found, null otherwise</returns>
    public DeepSidRecord? SearchByPath(string hvscPath)
    {
        Records.TryGetValue(hvscPath, out var record);
        return record;
    }

    private static string GetDeepSidFilePath()
    {
        var currentDirectory = Path.GetDirectoryName(typeof(DeepSidDatabase).Assembly.Location);

        if (currentDirectory is null) return string.Empty;

        // Look for exported JSON in the DeepSidDB directory
        var exportedDbPath = Path.Combine(currentDirectory, MusicConstants.DeepSid_Db_Local_Path, "deepsid_db.json");
        
        return File.Exists(exportedDbPath) ? exportedDbPath : string.Empty;
    }

    private static List<DeepSidRecord> ReadDeepSidJson(string filePath)
    {
        var json = File.ReadAllText(filePath);
        var files = JsonSerializer.Deserialize<List<DeepSidRecord>>(json, _jsonOptions);

        if (files == null)
        {
            throw new InvalidOperationException("Failed to deserialize DeepSID database");
        }

        return files;
    }

    private static Dictionary<string, DeepSidRecord> IndexByFilePath(List<DeepSidRecord> records)
    {
        return records
            .Where(r => !string.IsNullOrWhiteSpace(r.FilePath))
            .ToDictionary(r => r.FilePath, StringComparer.OrdinalIgnoreCase);
    }
}
