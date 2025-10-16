namespace TeensyRom.Tools.DeepSidExporter.Services.Transformation;

/// <summary>
/// Service for building URLs from database values
/// </summary>
public interface IUrlBuilderService
{
    /// <summary>
    /// Build YouTube URL from video ID
    /// </summary>
    string? BuildYouTubeUrl(string? videoId);

    /// <summary>
    /// Build CSDB URL from type and ID
    /// </summary>
    string? BuildCsdbUrl(string? csdbType, int? csdbId);
}