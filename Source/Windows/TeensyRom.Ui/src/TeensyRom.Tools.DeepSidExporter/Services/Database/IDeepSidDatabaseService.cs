using TeensyRom.Tools.DeepSidExporter.Models.Database;

namespace TeensyRom.Tools.DeepSidExporter.Services.Database;

/// <summary>
/// Service for accessing DeepSID MySQL database
/// </summary>
public interface IDeepSidDatabaseService
{
    /// <summary>
    /// Load all HVSC files from the main table
    /// </summary>
    Task<List<HvscFileDbModel>> LoadHvscFilesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Load all composers
    /// </summary>
    Task<List<ComposerDbModel>> LoadComposersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Load all composer links
    /// </summary>
    Task<List<ComposerLinkDbModel>> LoadComposerLinksAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Load all file tags with their types
    /// </summary>
    Task<List<TagDbModel>> LoadTagsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Load all YouTube videos
    /// </summary>
    Task<List<YoutubeDbModel>> LoadYoutubeVideosAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Load all competitions
    /// </summary>
    Task<List<CompetitionDbModel>> LoadCompetitionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Test database connectivity
    /// </summary>
    Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);
}