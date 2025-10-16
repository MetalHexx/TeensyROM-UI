using Microsoft.Extensions.Options;
using TeensyRom.Tools.DeepSidExporter.Configuration;

namespace TeensyRom.Tools.DeepSidExporter.Services.Transformation;

/// <summary>
/// Service for HVSC path cleaning and transformation
/// </summary>
public class PathTransformationService : IPathTransformationService
{
    private readonly TransformationConfig _config;

    public PathTransformationService(IOptions<TransformationConfig> config)
    {
        _config = config.Value;
    }

    /// <summary>
    /// Clean HVSC path by removing prefix and ensuring leading slash
    /// </summary>
    public string CleanHvscPath(string? rawPath)
    {
        if (string.IsNullOrWhiteSpace(rawPath))
            return "/";

        // Remove the HVSC prefix if present
        var cleaned = rawPath.Replace(_config.HvscPathPrefix, "", StringComparison.OrdinalIgnoreCase);

        // Ensure leading slash
        if (!cleaned.StartsWith('/'))
            cleaned = '/' + cleaned;

        return cleaned;
    }

    /// <summary>
    /// Check if file path matches composer path hierarchy
    /// </summary>
    public bool IsComposerPathMatch(string filePath, string composerPath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || string.IsNullOrWhiteSpace(composerPath))
            return false;

        // Check if composer path contains MUSICIANS and file is under composer's directory
        return composerPath.Contains("MUSICIANS", StringComparison.OrdinalIgnoreCase) && 
               filePath.StartsWith(composerPath + "/", StringComparison.OrdinalIgnoreCase);
    }
}