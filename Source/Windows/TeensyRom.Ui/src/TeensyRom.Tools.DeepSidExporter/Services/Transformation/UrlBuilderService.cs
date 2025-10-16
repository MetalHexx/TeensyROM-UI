namespace TeensyRom.Tools.DeepSidExporter.Services.Transformation;

/// <summary>
/// Service for building URLs from database values
/// </summary>
public class UrlBuilderService : IUrlBuilderService
{
    /// <summary>
    /// Build YouTube URL from video ID
    /// </summary>
    public string? BuildYouTubeUrl(string? videoId)
    {
        return !string.IsNullOrWhiteSpace(videoId) 
            ? $"https://www.youtube.com/watch?v={videoId}" 
            : null;
    }

    /// <summary>
    /// Build CSDB URL from type and ID
    /// </summary>
    public string? BuildCsdbUrl(string? csdbType, int? csdbId)
    {
        return !string.IsNullOrWhiteSpace(csdbType) && csdbId.HasValue && csdbId > 0
            ? $"https://csdb.dk/{csdbType}/?id={csdbId}"
            : null;
    }
}