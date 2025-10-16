using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySqlConnector;
using TeensyRom.Tools.DeepSidExporter.Configuration;
using TeensyRom.Tools.DeepSidExporter.Models.Database;
using TeensyRom.Tools.DeepSidExporter.Services.Infrastructure;

namespace TeensyRom.Tools.DeepSidExporter.Services.Database;

/// <summary>
/// Service for accessing DeepSID MySQL database
/// </summary>
public class DeepSidDatabaseService : IDeepSidDatabaseService
{
    private readonly DatabaseConfig _config;
    private readonly ILogger<DeepSidDatabaseService> _logger;
    private readonly IProgressReporter _progress;
    private const int BatchSize = 100;

    public DeepSidDatabaseService(
        IOptions<DatabaseConfig> config, 
        ILogger<DeepSidDatabaseService> logger,
        IProgressReporter progress)
    {
        _config = config.Value;
        _logger = logger;
        _progress = progress;
    }

    /// <summary>
    /// Safely get date string from MySQL reader, handling invalid dates
    /// </summary>
    private static string SafeGetDateString(MySqlDataReader reader, int ordinal)
    {
        try
        {
            if (reader.IsDBNull(ordinal))
                return "0000-00-00";
                
            var dateTime = reader.GetDateTime(ordinal);
            return dateTime.ToString("yyyy-MM-dd");
        }
        catch (MySqlConnector.MySqlConversionException)
        {
            // Invalid date (like 0000-00-00), return as string
            return "0000-00-00";
        }
    }

    /// <summary>
    /// Safely get string from MySQL reader, handling type conversion issues
    /// </summary>
    private static string SafeGetString(MySqlDataReader reader, int ordinal)
    {
        try
        {
            if (reader.IsDBNull(ordinal))
                return string.Empty;

            return reader.GetString(ordinal);
        }
        catch (InvalidCastException)
        {
            // If that fails, try to convert the value to string
            try
            {
                var value = reader.GetValue(ordinal);
                return value?.ToString() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Safely get integer from MySQL reader, handling VarChar to Int32 conversion issues
    /// </summary>
    private static int SafeGetInt32(MySqlDataReader reader, int ordinal)
    {
        try
        {
            if (reader.IsDBNull(ordinal))
                return 0;

            // First try to get as int directly
            return reader.GetInt32(ordinal);
        }
        catch (InvalidCastException)
        {
            // If that fails, try to get as string and parse
            try
            {
                var value = reader.GetValue(ordinal);
                return int.TryParse(value?.ToString(), out var result) ? result : 0;
            }
            catch
            {
                return 0;
            }
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Load all HVSC files from the main table
    /// </summary>
    public async Task<List<HvscFileDbModel>> LoadHvscFilesAsync(CancellationToken cancellationToken = default)
    {
        using var connection = new MySqlConnection(_config.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        // Get total count first
        using var countCommand = new MySqlCommand("SELECT COUNT(*) FROM hvsc_files", connection);
        var totalCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync(cancellationToken));

        const int batchSize = 100; // Small batch size for stability
        const string query = """
            SELECT id,fullname,player,lengths,type,version,playertype,playercompat,clockspeed,sidmodel,dataoffset,datasize,loadaddr,initaddr,playaddr,subtunes,startsubtune,name,author,copyright,hash,stil,new,updated,csdbtype,csdbid,application,gb64 
            FROM hvsc_files
            WHERE id > @LastId
            ORDER BY id
            LIMIT @Limit
            """;

        var files = new List<HvscFileDbModel>(totalCount);
        var lastId = 0;

        while (files.Count < totalCount)
        {
            using var command = new MySqlCommand(query, connection)
            {
                CommandTimeout = _config.CommandTimeout
            };
            command.Parameters.AddWithValue("@LastId", lastId);
            command.Parameters.AddWithValue("@Limit", batchSize);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            
            var batchCount = 0;

            while (await reader.ReadAsync(cancellationToken))
            {
                var fileId = SafeGetInt32(reader, 0);
                files.Add(new HvscFileDbModel
                {
                    Id = fileId,                          // id (0)
                    Fullname = SafeGetString(reader, 1),  // fullname (1)
                    Player = SafeGetString(reader, 2),    // player (2)
                    Lengths = SafeGetString(reader, 3),   // lengths (3)
                    Type = SafeGetString(reader, 4),      // type (4)
                    Version = SafeGetString(reader, 5),   // version (5)
                    PlayerType = SafeGetString(reader, 6), // playertype (6)
                    PlayerCompat = SafeGetString(reader, 7), // playercompat (7)
                    ClockSpeed = SafeGetString(reader, 8), // clockspeed (8)
                    SidModel = SafeGetString(reader, 9),  // sidmodel (9)
                    DataOffset = SafeGetInt32(reader, 10), // dataoffset (10)
                    DataSize = SafeGetInt32(reader, 11),   // datasize (11)
                    LoadAddr = SafeGetInt32(reader, 12),   // loadaddr (12)
                    InitAddr = SafeGetInt32(reader, 13),   // initaddr (13)
                    PlayAddr = SafeGetInt32(reader, 14),   // playaddr (14)
                    Subtunes = SafeGetInt32(reader, 15),   // subtunes (15)
                    StartSubtune = SafeGetInt32(reader, 16), // startsubtune (16)
                    Name = SafeGetString(reader, 17),     // name (17)
                    Author = SafeGetString(reader, 18),   // author (18)
                    Copyright = SafeGetString(reader, 19), // copyright (19)
                    Hash = SafeGetString(reader, 20),     // hash (20)
                    Stil = SafeGetString(reader, 21),     // stil (21)
                    New = SafeGetInt32(reader, 22),       // new (22)
                    Updated = SafeGetInt32(reader, 23),   // updated (23)
                    CsdbType = SafeGetString(reader, 24), // csdbtype (24)
                    CsdbId = SafeGetInt32(reader, 25),    // csdbid (25)
                    Application = SafeGetString(reader, 26), // application (26)
                    Gb64 = SafeGetString(reader, 27)      // gb64 (27)
                });
                lastId = fileId; // Track last ID for next iteration
                batchCount++;
            }

            var percent = Math.Min(100, (int)((double)files.Count / totalCount * 100));
            _progress.Report($"  [1/6] Loading HVSC files... {files.Count:N0} / {totalCount:N0} ({percent}%)");

            if (batchCount == 0)
            {
                break; // No more records
            }
        }

        return files;
    }

    /// <summary>
    /// Load all composers
    /// </summary>
    public async Task<List<ComposerDbModel>> LoadComposersAsync(CancellationToken cancellationToken = default)
    {
        // Loading composers (progress updated in loop)
        
        using var connection = new MySqlConnection(_config.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        // Get total count
        using var countCommand = new MySqlCommand("SELECT COUNT(*) FROM composers", connection);
        var totalCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync(cancellationToken));
        // Total count retrieved

        const int batchSize = 100;
        const string query = """
            SELECT id,fullname,name,shortname,handles,shorthandle,active,born,died,country,csdbtype,csdbid,imagesource,notable,employment,affiliation,brand,branddark 
            FROM composers
            WHERE id > @LastId
            ORDER BY id
            LIMIT @Limit
            """;

        var composers = new List<ComposerDbModel>(totalCount);
        var lastId = 0;

        while (composers.Count < totalCount)
        {
            using var command = new MySqlCommand(query, connection)
            {
                CommandTimeout = _config.CommandTimeout
            };
            command.Parameters.AddWithValue("@LastId", lastId);
            command.Parameters.AddWithValue("@Limit", batchSize);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var batchCount = 0;

            while (await reader.ReadAsync(cancellationToken))
            {
                var composerId = SafeGetInt32(reader, 0);
                composers.Add(new ComposerDbModel
                {
                    Id = composerId,                      // id (safe conversion)
                    Fullname = reader.GetString(1),       // fullname
                    Name = reader.GetString(2),           // name
                    Shortname = reader.GetString(3),      // shortname
                    Handles = reader.GetString(4),        // handles
                    Shorthandle = reader.GetString(5),    // shorthandle
                    Active = SafeGetInt32(reader, 6),     // active (year - safe conversion)
                    Born = SafeGetDateString(reader, 7),  // born (safe date conversion)
                    Died = SafeGetDateString(reader, 8),  // died (safe date conversion)
                    Country = reader.GetString(9),        // country
                    CsdbType = reader.GetString(10),      // csdbtype
                    CsdbId = SafeGetInt32(reader, 11),    // csdbid (safe conversion)
                    ImageSource = reader.GetString(12),   // imagesource (enum)
                    Notable = reader.GetString(13),       // notable
                    Employment = reader.GetString(14),    // employment
                    Affiliation = reader.GetString(15),   // affiliation
                    Brand = reader.GetString(16),         // brand
                    BrandDark = reader.GetString(17)      // branddark
                });
                lastId = composerId;
                batchCount++;
            }

            var percent = Math.Min(100, (int)((double)composers.Count / totalCount * 100));
            _progress.Report($"  [2/6] Loading composers... {composers.Count:N0} / {totalCount:N0} ({percent}%)");

            if (batchCount == 0)
                break;
        }

        // Completed
        return composers;
    }

    /// <summary>
    /// Load all composer links
    /// </summary>
    public async Task<List<ComposerLinkDbModel>> LoadComposerLinksAsync(CancellationToken cancellationToken = default)
    {
        // Loading composer links
        
        using var connection = new MySqlConnection(_config.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        // Get total count
        using var countCommand = new MySqlCommand("SELECT COUNT(*) FROM composers_links", connection);
        var totalCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync(cancellationToken));        const int batchSize = 100;
        const string query = "SELECT composers_id,name,url FROM composers_links WHERE composers_id > @LastId ORDER BY composers_id LIMIT @Limit";

        var links = new List<ComposerLinkDbModel>(totalCount);
        var lastId = 0;

        while (links.Count < totalCount)
        {
            using var command = new MySqlCommand(query, connection)
            {
                CommandTimeout = _config.CommandTimeout
            };
            command.Parameters.AddWithValue("@LastId", lastId);
            command.Parameters.AddWithValue("@Limit", batchSize);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var batchCount = 0;

            while (await reader.ReadAsync(cancellationToken))
            {
                var composerId = SafeGetInt32(reader, 0);
                links.Add(new ComposerLinkDbModel
                {
                    ComposersId = composerId,              // composers_id (safe conversion)
                    Name = reader.GetString(1),            // name
                    Url = reader.GetString(2)              // url
                });
                lastId = composerId;
                batchCount++;
            }

            var percent = Math.Min(100, (int)((double)links.Count / totalCount * 100));
            _progress.Report($"  [3/6] Loading composer links... {links.Count:N0} / {totalCount:N0} ({percent}%)");

            if (batchCount == 0)
                break;
        }        return links;
    }

    /// <summary>
    /// Load all file tags with their types
    /// </summary>
    public async Task<List<TagDbModel>> LoadTagsAsync(CancellationToken cancellationToken = default)
    {
        // Loading tags
        
        using var connection = new MySqlConnection(_config.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        // Get total count
        using var countCommand = new MySqlCommand("SELECT COUNT(*) FROM tags_lookup", connection);
        var totalCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync(cancellationToken));        const int batchSize = 100;
        const string query = """
            SELECT tl.files_id, ti.name, ti.type 
            FROM tags_lookup tl 
            JOIN tags_info ti ON tl.tags_id = ti.id
            WHERE tl.files_id > @LastId
            ORDER BY tl.files_id
            LIMIT @Limit
            """;

        var tags = new List<TagDbModel>(totalCount);
        var lastId = 0;

        while (tags.Count < totalCount)
        {
            using var command = new MySqlCommand(query, connection)
            {
                CommandTimeout = _config.CommandTimeout
            };
            command.Parameters.AddWithValue("@LastId", lastId);
            command.Parameters.AddWithValue("@Limit", batchSize);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var batchCount = 0;

            while (await reader.ReadAsync(cancellationToken))
            {
                var fileId = SafeGetInt32(reader, 0);
                tags.Add(new TagDbModel
                {
                    FileId = fileId,                       // files_id (safe conversion)
                    Name = reader.GetString(1),            // name
                    Type = reader.IsDBNull(2) ? "" : reader.GetString(2)  // type
                });
                lastId = fileId;
                batchCount++;
            }

            var percent = Math.Min(100, (int)((double)tags.Count / totalCount * 100));
            _progress.Report($"  [4/6] Loading tags... {tags.Count:N0} / {totalCount:N0} ({percent}%)");

            if (batchCount == 0)
                break;
        }        return tags;
    }

    /// <summary>
    /// Load all YouTube videos
    /// </summary>
    public async Task<List<YoutubeDbModel>> LoadYoutubeVideosAsync(CancellationToken cancellationToken = default)
    {
        // Loading YouTube
        
        using var connection = new MySqlConnection(_config.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        // Get total count
        using var countCommand = new MySqlCommand("SELECT COUNT(*) FROM youtube", connection);
        var totalCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync(cancellationToken));        const int batchSize = 100;
        const string query = "SELECT file_id,video_id,subtune,channel FROM youtube WHERE file_id > @LastId ORDER BY file_id LIMIT @Limit";

        var videos = new List<YoutubeDbModel>(totalCount);
        var lastId = 0;

        while (videos.Count < totalCount)
        {
            using var command = new MySqlCommand(query, connection)
            {
                CommandTimeout = _config.CommandTimeout
            };
            command.Parameters.AddWithValue("@LastId", lastId);
            command.Parameters.AddWithValue("@Limit", batchSize);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var batchCount = 0;

            while (await reader.ReadAsync(cancellationToken))
            {
                var fileId = SafeGetInt32(reader, 0);
                videos.Add(new YoutubeDbModel
                {
                    FileId = fileId,                       // file_id (safe conversion)
                    VideoId = reader.GetString(1),         // video_id
                    Subtune = SafeGetInt32(reader, 2),     // subtune (safe conversion)
                    Channel = reader.GetString(3)          // channel
                });
                lastId = fileId;
                batchCount++;
            }

            var percent = Math.Min(100, (int)((double)videos.Count / totalCount * 100));
            _progress.Report($"  [5/6] Loading YouTube videos... {videos.Count:N0} / {totalCount:N0} ({percent}%)");

            if (batchCount == 0)
                break;
        }        return videos;
    }

    /// <summary>
    /// Load all competitions
    /// </summary>
    public async Task<List<CompetitionDbModel>> LoadCompetitionsAsync(CancellationToken cancellationToken = default)
    {
        // Loading competitions
        
        using var connection = new MySqlConnection(_config.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        // Get total count
        using var countCommand = new MySqlCommand("SELECT COUNT(*) FROM competitions_cache", connection);
        var totalCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync(cancellationToken));        const int batchSize = 100;
        const string query = "SELECT file_id,name,place FROM competitions_cache WHERE file_id > @LastId ORDER BY file_id LIMIT @Limit";

        var competitions = new List<CompetitionDbModel>(totalCount);
        var lastId = 0;

        while (competitions.Count < totalCount)
        {
            using var command = new MySqlCommand(query, connection)
            {
                CommandTimeout = _config.CommandTimeout
            };
            command.Parameters.AddWithValue("@LastId", lastId);
            command.Parameters.AddWithValue("@Limit", batchSize);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var batchCount = 0;

            while (await reader.ReadAsync(cancellationToken))
            {
                var fileId = SafeGetInt32(reader, 0);
                competitions.Add(new CompetitionDbModel
                {
                    FileId = fileId,                       // file_id (safe conversion)
                    Name = reader.GetString(1),            // name
                    Place = SafeGetInt32(reader, 2)        // place (safe conversion)
                });
                lastId = fileId;
                batchCount++;
            }

            var percent = Math.Min(100, (int)((double)competitions.Count / totalCount * 100));
            _progress.Report($"  [6/6] Loading competitions... {competitions.Count:N0} / {totalCount:N0} ({percent}%)");

            if (batchCount == 0)
                break;
        }

        // Completed
        return competitions;
    }

    /// <summary>
    /// Test database connectivity
    /// </summary>
    public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Testing connection
            
            using var connection = new MySqlConnection(_config.ConnectionString);
            await connection.OpenAsync(cancellationToken);
            
            // Get database info
            var serverVersion = connection.ServerVersion;
            var database = connection.Database;
            
            // Test with a simple query to get table count
            using var command = new MySqlCommand("SELECT COUNT(*) FROM hvsc_files", connection);
            var count = await command.ExecuteScalarAsync(cancellationToken);            // Check for indexes on key columns
            await VerifyIndexesAsync(connection, cancellationToken);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database connection failed: {Message}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Verify that indexes exist on key columns for performance
    /// </summary>
    private async Task VerifyIndexesAsync(MySqlConnection connection, CancellationToken cancellationToken)
    {
        try
        {
            var tables = new[]
            {
                ("hvsc_files", "id"),
                ("composers", "id"),
                ("composers_links", "composers_id"),
                ("tags_lookup", "files_id"),
                ("youtube", "file_id"),
                ("competitions_cache", "file_id")
            };

            foreach (var (table, column) in tables)
            {
                var indexQuery = $"""
                    SELECT COUNT(*) 
                    FROM information_schema.statistics 
                    WHERE table_schema = DATABASE() 
                    AND table_name = '{table}' 
                    AND column_name = '{column}'
                    """;

                using var cmd = new MySqlCommand(indexQuery, connection);
                var indexCount = Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken));
                
                if (indexCount == 0)
                {
                    _logger.LogWarning("Missing index on {Table}.{Column} - performance may be degraded", table, column);
                }
                else
                {
                    _logger.LogDebug("Index verified on {Table}.{Column}", table, column);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not verify indexes - continuing anyway");
        }
    }
}
