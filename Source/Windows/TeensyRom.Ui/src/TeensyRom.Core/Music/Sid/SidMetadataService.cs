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
                EnrichWithHsvcMetadata(song, sidRecord);
                EnrichWithHsvcMusicianImage(song);
            }
            return song;
        }

        private void EnrichWithHsvcMetadata(SongItem song, SidRecord? sidRecord)
        {
            if (sidRecord is null) return;

            song.Creator = sidRecord.Author;
            song.Title = sidRecord.Title;
            song.SongLength = sidRecord.SongLengthSpan;
            song.ReleaseInfo = sidRecord.Released;
            song.Description = CleanDescription(sidRecord.StilEntry);
            song.MetadataSource = SidConstants.Hvsc;
            song.ShareUrl = $"https://deepsid.chordian.net/?file={sidRecord.Filepath}";
        }

        private void EnrichWithHsvcMusicianImage(SongItem song) 
        {
            var currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var remainingPathSegments = song.Path.GetRemainingPathSegments(SidConstants.Hvsc_Musician_Base_Remote_Path);

            var hsvcImageName = $"{Path.Combine(currentDirectory!, SidConstants.Musician_Image_Local_Path)}musicians";

            foreach(var segment in remainingPathSegments)
            {
                hsvcImageName = $"{hsvcImageName}_{segment}";
            }
            hsvcImageName = $"{hsvcImageName}.jpg";

            if(File.Exists(hsvcImageName))
            {
                song.Images.Add(new ViewableItemImage
                {
                    Path = hsvcImageName,
                    Source = SidConstants.DeepSid
                });
            }
        }

        public string CleanDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description)) return string.Empty;

            var cleanedDescription = $"{description.StripCarriageReturnsAndExtraWhitespace()}";
            cleanedDescription = cleanedDescription.Replace("COMMENT:", "\r\nComment:\r\n");
            cleanedDescription = cleanedDescription.Replace("ARTIST:", "\r\nArtist: ");
            cleanedDescription = cleanedDescription.Replace("TITLE:", "\r\nTitle: ");
            cleanedDescription = cleanedDescription.RemoveFirstOccurrence("\r\n");
            return cleanedDescription;
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
                r.Filepath = r.Filename;
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