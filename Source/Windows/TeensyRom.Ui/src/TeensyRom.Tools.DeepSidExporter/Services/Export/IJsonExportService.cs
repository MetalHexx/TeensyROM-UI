using TeensyRom.Tools.DeepSidExporter.Models.Export;

namespace TeensyRom.Tools.DeepSidExporter.Services.Export;

/// <summary>
/// Service for JSON serialization and export
/// </summary>
public interface IJsonExportService
{
    /// <summary>
    /// Export HVSC files to JSON file
    /// </summary>
    Task ExportToJsonAsync(List<HvscFile> files, string outputPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate JSON file structure
    /// </summary>
    Task<bool> ValidateJsonAsync(string filePath, CancellationToken cancellationToken = default);
}