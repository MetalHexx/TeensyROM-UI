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

        public SongItem EnrichSong(SongItem song)
        {
            _songDatabase.TryGetValue(song.Id, out var sidRecord);

            if (sidRecord is not null)
            {
                EnrichWithHsvcMetadata(song, sidRecord);                
            }
            EnrichWithHsvcMusicianImage(song);
            return song;
        }

        private void EnrichWithHsvcMetadata(SongItem song, SidRecord? sidRecord)
        {
            if (sidRecord is null) return;

            var cleanedSTIL = CleanDescription(sidRecord.StilEntry);
            var missingSTIL = string.IsNullOrWhiteSpace(cleanedSTIL);

            song.Description = missingSTIL ? song.Description : cleanedSTIL;
            song.MetadataSource = missingSTIL ? string.Empty : MusicConstants.Hvsc;
            song.Creator = sidRecord.Author;
            song.Title = sidRecord.Title;
            song.PlayLength = sidRecord.SongLengthSpan;
            song.ReleaseInfo = sidRecord.Released;            
            song.Meta1 = sidRecord.Clock;
            song.Meta2 = sidRecord.SidModel;            
            
            song.MetadataSourcePath = song.MetadataSourcePath.RemoveLeadingAndTrailingSlash().Contains(sidRecord.Filepath.RemoveLeadingAndTrailingSlash()) 
                ? song.MetadataSourcePath 
                : sidRecord.Filepath;
            
            song.ShareUrl = $"https://deepsid.chordian.net/?file={sidRecord.Filepath}";
        }

        /// <summary>
        /// Assigns a composer image to a sid
        /// </summary>
        /// <remarks>
        /// To add a new composer image for a composer not in the HVSC:
        /// Add SID in the HVSC /musicians/<letter> folder structure 
        /// Add composer image with musicians_<letter>_<musicianname>.jpg
        /// TODO: Make more generic and remove HVSC folder structure requirement
        /// </remarks>
        /// <param name="song"></param>
        private void EnrichWithHsvcMusicianImage(SongItem song) 
        {
            var currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var songPath = string.IsNullOrWhiteSpace(song.MetadataSourcePath) ? song.Path : song.MetadataSourcePath;

            var remainingPathSegments = songPath.GetRemainingPathSegments(MusicConstants.Hvsc_Musician_Base_Remote_Path);

            var hsvcImageName = $"{Path.Combine(currentDirectory!, MusicConstants.Musician_Image_Local_Path)}musicians";

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
                    Source = MusicConstants.DeepSid
                });
            }
        }

        public string CleanDescription(string stilDescription)
        {
            if (string.IsNullOrWhiteSpace(stilDescription)) return string.Empty;

            var cleanedDescription = $"{stilDescription.StripCarriageReturnsAndExtraWhitespace()}";
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
                var songLengths = ParseTimeSegments(sid.Value.SongLength);
                
                sid.Value.SongLengthSpan = songLengths.Any() ? songLengths.First() : MusicConstants.DefaultLength;
            }
            return sids;
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
            catch { }

            return timeSpans;
        }

        public void Dispose()
        {
            _settingsSubscription?.Dispose();
        }
    }
}