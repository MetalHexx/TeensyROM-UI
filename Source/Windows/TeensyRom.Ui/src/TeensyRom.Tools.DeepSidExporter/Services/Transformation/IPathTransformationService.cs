namespace TeensyRom.Tools.DeepSidExporter.Services.Transformation;

/// <summary>
/// Service for HVSC path cleaning and transformation
/// </summary>
public interface IPathTransformationService
{
    /// <summary>
    /// Clean HVSC path by removing prefix and ensuring leading slash
    /// </summary>
    string CleanHvscPath(string? rawPath);

    /// <summary>
    /// Check if file path matches composer path hierarchy
    /// </summary>
    bool IsComposerPathMatch(string filePath, string composerPath);
}