using System.Text.Json.Serialization;

namespace TeensyRom.Core.Music.DeepSid;

/// <summary>
/// Represents a complete HVSC file with all denormalized relationships
/// </summary>
public class DeepSidRecord
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("file_path")]
    public string FilePath { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("author")]
    public string Author { get; set; } = string.Empty;

    [JsonPropertyName("copyright")]
    public string Copyright { get; set; } = string.Empty;

    [JsonPropertyName("composer")]
    public DeepSidComposer? Composer { get; set; }

    [JsonPropertyName("player_type")]
    public string PlayerType { get; set; } = string.Empty;

    [JsonPropertyName("subtune_lengths")]
    public string SubtuneLengths { get; set; } = string.Empty;

    [JsonPropertyName("file_type")]
    public string FileType { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public int Version { get; set; }

    [JsonPropertyName("playertype")]
    public string PlayerTypeRaw { get; set; } = string.Empty;

    [JsonPropertyName("playercompat")]
    public string PlayerCompat { get; set; } = string.Empty;

    [JsonPropertyName("clockspeed")]
    public string ClockSpeed { get; set; } = string.Empty;

    [JsonPropertyName("sidmodel")]
    public string SidModel { get; set; } = string.Empty;

    [JsonPropertyName("dataoffset")]
    public int DataOffset { get; set; }

    [JsonPropertyName("datasize")]
    public int DataSize { get; set; }

    [JsonPropertyName("loadaddr")]
    public int LoadAddr { get; set; }

    [JsonPropertyName("initaddr")]
    public int InitAddr { get; set; }

    [JsonPropertyName("playaddr")]
    public int PlayAddr { get; set; }

    [JsonPropertyName("subtunes")]
    public int Subtunes { get; set; }

    [JsonPropertyName("startsubtune")]
    public int StartSubtune { get; set; }

    [JsonPropertyName("hash")]
    public string Hash { get; set; } = string.Empty;

    [JsonPropertyName("stil")]
    public string? Stil { get; set; }

    [JsonPropertyName("is_new")]
    public int IsNew { get; set; }

    [JsonPropertyName("last_updated")]
    public int LastUpdated { get; set; }

    [JsonPropertyName("csdbtype")]
    public string? CsdbType { get; set; }

    [JsonPropertyName("csdbid")]
    public int? CsdbId { get; set; }

    [JsonPropertyName("csdb_url")]
    public string? CsdbUrl { get; set; }

    [JsonPropertyName("application")]
    public string Application { get; set; } = string.Empty;

    [JsonPropertyName("gb64")]
    public string Gb64 { get; set; } = string.Empty;

    [JsonPropertyName("tags")]
    public List<DeepSidTag> Tags { get; set; } = new();

    [JsonPropertyName("youtube_videos")]
    public List<DeepSidYouTubeVideo> YouTubeVideos { get; set; } = new();

    [JsonPropertyName("competitions")]
    public List<DeepSidCompetition> Competitions { get; set; } = new();

    [JsonPropertyName("avg_rating")]
    public double? AvgRating { get; set; }

    [JsonPropertyName("rating_count")]
    public int RatingCount { get; set; }
}
