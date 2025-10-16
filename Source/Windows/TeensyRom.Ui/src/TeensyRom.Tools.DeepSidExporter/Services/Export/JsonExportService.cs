using System.Text.Json;
using Microsoft.Extensions.Logging;
using TeensyRom.Tools.DeepSidExporter.Models.Export;

namespace TeensyRom.Tools.DeepSidExporter.Services.Export;

/// <summary>
/// Service for JSON serialization and export
/// </summary>
public class JsonExportService : IJsonExportService
{
    private readonly ILogger<JsonExportService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false, // Compact JSON like PowerShell script
        PropertyNamingPolicy = null, // Use exact property names as defined
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never
    };

    public JsonExportService(ILogger<JsonExportService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Export HVSC files to JSON file
    /// </summary>
    public async Task ExportToJsonAsync(List<HvscFile> files, string outputPath, CancellationToken cancellationToken = default)
    {
        try
        {
            // Ensure output directory exists
            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Serialize to JSON
            var json = JsonSerializer.Serialize(files, JsonOptions);

            // Write to file
            await File.WriteAllTextAsync(outputPath, json, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export JSON to {OutputPath}", outputPath);
            throw;
        }
    }

    /// <summary>
    /// Validate JSON file structure
    /// </summary>
    public async Task<bool> ValidateJsonAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                _logger.LogError("JSON file not found: {FilePath}", filePath);
                return false;
            }

            var json = await File.ReadAllTextAsync(filePath, cancellationToken);
            var files = JsonSerializer.Deserialize<List<HvscFile>>(json, JsonOptions);

            if (files == null)
            {
                _logger.LogError("JSON deserialization returned null");
                return false;
            }

            return true;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON validation failed - Invalid JSON structure");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "JSON validation failed");
            return false;
        }
    }
}