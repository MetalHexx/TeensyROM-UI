using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Runtime;
using System.Security.Cryptography;
using System.Text;
using TeensyRom.Core.Common;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Music.Sid
{
    public interface ISidMetadataService
    {
        SongItem EnrichSong(SongItem song);
    }

    public class SidMetadataService : ISidMetadataService, IDisposable
    {
        private readonly string _filePath;
        private readonly Dictionary<string, SidRecord> _songDatabase = new();
        private TeensySettings _settings = new();
        private readonly ISettingsService _settingsService;
        private IDisposable _settingsSubscription;

        public SidMetadataService(ISettingsService settingsService)
        {
            _filePath = GetSidFilePath();
            _songDatabase = ParseSids(ReadCsv());

            _settingsService = settingsService;

            _settingsSubscription = _settingsService.Settings.Subscribe(OnSettingsChanged);
        }

        private void OnSettingsChanged(TeensySettings settings)
        {
            _settings = settings;
        }

        private static string GetSidFilePath()
        {
            var currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (currentDirectory is null) return string.Empty;

            var relativePath = @"Music\Sid\SIDlist_79_UTF8.csv";

            return Path.Combine(currentDirectory, relativePath);
        }

        public SongItem EnrichSong(SongItem song)
        {
            _songDatabase.TryGetValue(song.Id, out var sidRecord);

            if (sidRecord is not null)
            {
                song.ArtistName = sidRecord.Author;
                song.Name = sidRecord.Title;
                song.SongLength = sidRecord.SongLengthSpan;
                song.ReleaseInfo = sidRecord.Released;
                song.Comments = sidRecord.StilEntry;
            }
            return song;
        }

        private Dictionary<string, SidRecord> ReadCsv()
        {
            if (string.IsNullOrWhiteSpace(_filePath)) return _songDatabase;

            using var reader = new StreamReader(_filePath, Encoding.UTF8);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ",",
                IgnoreBlankLines = true,
                MissingFieldFound = null
            });

            csv.Context.RegisterClassMap<SidRecordMap>();
            var records = csv.GetRecords<SidRecord>();
            return FilterOutDupes(records);
        }

        private Dictionary<string, SidRecord> FilterOutDupes(IEnumerable<SidRecord> records) => records
            .Select(r =>
            {
                r.Filename = r.Filename.GetFileNameFromPath();
                return r;
            })
            .GroupBy(r => new { r.SizeInBytes, r.Filename })
            .Where(g => g.Count() == 1)
            .SelectMany(g => g)
            .ToList()
            .ToDictionary(r => $"{r.SizeInBytes}{r.Filename.GetFileNameFromPath()}");

        private static Dictionary<string, SidRecord> ParseSids(Dictionary<string, SidRecord> sids)
        {
            foreach (var sid in sids)
            {
                sid.Value.Title = sid.Value.Title.EnsureNotEmpty(sid.Value.Filename.GetFileNameFromPath());
                sid.Value.Author = sid.Value.Author.EnsureNotEmpty("Unknown Artist");
                sid.Value.Released = sid.Value.Released.EnsureNotEmpty("No Release Info");
                sid.Value.StilEntry = sid.Value.StilEntry.EnsureNotEmpty("No Comments");

                if (string.IsNullOrEmpty(sid.Value.SongLength))
                {
                    sid.Value.SongLengthSpan = MusicConstants.DefaultLength;
                    continue;
                }

                var timeSpanFormats = new[] { @"m\:ss", @"mm\:ss", @"m\:ss\.f", @"mm\:ss\.f", @"m\:ss\.ff", @"mm\:ss\.ff" };

                if (TimeSpan.TryParseExact(sid.Value.SongLength.Trim(), timeSpanFormats, CultureInfo.InvariantCulture, TimeSpanStyles.None, out var timeSpan))
                {
                    sid.Value.SongLengthSpan = timeSpan;
                    continue;
                }
                sid.Value.SongLengthSpan = MusicConstants.DefaultLength;
            }
            return sids;
        }

        public void Dispose()
        {
            _settingsSubscription?.Dispose();
        }
    }
}