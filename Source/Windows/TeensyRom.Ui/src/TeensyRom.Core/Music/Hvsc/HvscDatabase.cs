using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Reflection;
using System.Text;
using TeensyRom.Core.Common;
using TeensyRom.Core.Music.Sid;

namespace TeensyRom.Core.Music.Hvsc;

/// <summary>
/// Provides access to the HVSC database from CSV files
/// </summary>
public class HvscDatabase : IHvscDatabase
{
    private static readonly CsvConfiguration _csvConfig = new(CultureInfo.InvariantCulture)
    {
        HasHeaderRecord = true,
        Delimiter = ",",
        IgnoreBlankLines = true,
        MissingFieldFound = null
    };

    /// <summary>
    /// All HVSC SID records indexed by their file ID (Size + FileName)
    /// </summary>
    private Dictionary<string, SidRecord> Records { get; set; } = new();

    /// <summary>
    /// Creates a new HvscDatabase instance and loads data from the specified CSV file
    /// </summary>
    /// <param name="csvFilePath">Path to the HVSC CSV file. If null or empty, attempts to find CSV in default location.</param>
    public HvscDatabase(string? csvFilePath = null)
    {
        var filePath = csvFilePath ?? GetHvscFilePath();

        if (string.IsNullOrWhiteSpace(filePath))
        {
            Records = new Dictionary<string, SidRecord>();
            return;
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"HVSC CSV file not found: {filePath}");
        }

        var rawRecords = ReadHvscCsv(filePath);
        Records = NormalizeRecords(rawRecords);
    }

    /// <summary>
    /// Gets a SID record by its file ID (Size + FileName)
    /// </summary>
    /// <param name="fileId">The file ID (Size + FileName) for the SID record</param>
    /// <returns>SID record if found, null otherwise</returns>
    public SidRecord? GetRecord(string fileId)
    {
        Records.TryGetValue(fileId, out var record);
        return record;
    }

    private static string GetHvscFilePath()
    {
        var currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        if (currentDirectory is null) return string.Empty;

        var sidListPath = Path.Combine(currentDirectory, MusicConstants.SidList_Local_Path);

        if (!Directory.Exists(sidListPath)) return string.Empty;

        var csvFiles = Directory.GetFiles(sidListPath, "*.csv");

        if (csvFiles.Length == 0) return string.Empty;

        return csvFiles
            .Select(file => new FileInfo(file))
            .OrderByDescending(fi => fi.LastWriteTime)
            .First()
            .FullName;
    }

    private static Dictionary<string, SidRecord> ReadHvscCsv(string filePath)
    {
        using var reader = new StreamReader(filePath, Encoding.UTF8);
        using var csv = new CsvReader(reader, _csvConfig);

        csv.Context.RegisterClassMap<SidRecordMap>();
        var records = csv.GetRecords<SidRecord>();
        return FilterOutDuplicates(records);
    }

    private static Dictionary<string, SidRecord> FilterOutDuplicates(IEnumerable<SidRecord> records) => records
        .Select(r =>
        {
            r.Filepath = r.Filename;
            r.Filename = r.Filename.GetFileNameFromUnixPath();
            return r;
        })
        .GroupBy(r => $"{r.SizeInBytes}{r.Filename}") // Group by size+filename composite key
        .Where(g => g.Count() == 1)
        .SelectMany(g => g)
        .ToList()
        .ToDictionary(r => $"{r.SizeInBytes}{r.Filename}"); // Use size+filename as the key

    private static Dictionary<string, SidRecord> NormalizeRecords(Dictionary<string, SidRecord> records)
    {
        foreach (var kvp in records)
        {
            var record = kvp.Value;

            // Normalize metadata fields
            record.Title = record.Title.EnsureNotEmpty(record.Filename.GetFileNameFromUnixPath());
            record.Author = record.Author.EnsureNotEmpty("Unknown Artist");
            record.Released = record.Released.EnsureNotEmpty("No Release Info");

            // Clean STIL description
            record.StilEntry = CleanStilDescription(record.StilEntry);

            // Parse and normalize song lengths
            if (string.IsNullOrEmpty(record.SongLength))
            {
                record.SongLengthSpan = MusicConstants.DefaultLength;
                record.SubTuneSongLengths = [];
            }
            else
            {
                var songLengths = ParseTimeSegments(record.SongLength);
                record.SongLengthSpan = songLengths.Any() ? songLengths.First() : MusicConstants.DefaultLength;
                record.SubTuneSongLengths = songLengths;
            }
        }

        return records;
    }

    /// <summary>
    /// Cleans and formats STIL description text
    /// </summary>
    /// <param name="stilDescription">Raw STIL description</param>
    /// <returns>Cleaned and formatted STIL description</returns>
    private static string CleanStilDescription(string stilDescription)
    {
        if (string.IsNullOrWhiteSpace(stilDescription)) return string.Empty;

        var cleanedDescription = $"{stilDescription.StripCarriageReturnsAndExtraWhitespace()}";
        cleanedDescription = cleanedDescription.Replace("COMMENT:", "\r\nComment:\r\n");
        cleanedDescription = cleanedDescription.Replace("ARTIST:", "\r\nArtist: ");
        cleanedDescription = cleanedDescription.Replace("TITLE:", "\r\nTitle: ");
        cleanedDescription = cleanedDescription.RemoveFirstOccurrence("\r\n");
        return cleanedDescription;
    }

    private static List<TimeSpan> ParseTimeSegments(string input)
    {
        List<TimeSpan> timeSpans = [];

        try
        {
            input = input.Replace(".", ":");
            var segments = input.Split(' ');

            foreach (var segment in segments)
            {
                TimeSpan timeSpan;

                if (segment.Contains(':') && segment.Split(':').Length == 3)
                {
                    var parts = segment.Split(':');
                    int minutes = int.Parse(parts[0]);
                    int seconds = int.Parse(parts[1]);
                    int milliseconds = int.Parse(parts[2]);
                    timeSpan = new TimeSpan(0, 0, minutes, seconds, milliseconds);
                }
                else
                {
                    timeSpan = TimeSpan.ParseExact(segment, @"m\:ss", null);
                }
                timeSpans.Add(timeSpan);
            }
        }
        catch
        {
            // If parsing fails, return empty list
        }

        return timeSpans;
    }
}