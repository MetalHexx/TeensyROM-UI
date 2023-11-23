using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Music
{
    public class MusicService : IMusicService
    {
        private readonly string _filePath;
        private readonly Dictionary<string, SidRecord> _songDatabase = new();

        public MusicService()
        {
            _filePath = GetSidFilePath(); 
            _songDatabase = ParseSids(ReadCsv());
        }

        private static string GetSidFilePath()
        {
            var currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (currentDirectory is null) return string.Empty;

            var relativePath = @"Music\SIDlist_79_UTF8.csv";

            return Path.Combine(currentDirectory, relativePath);
        }

        public SongItem EnrichSong(SongItem song, string dbPath)
        {
            _songDatabase.TryGetValue(dbPath, out var record);

            if (record is not null)
            {
                song.ArtistName = record.Author;
                song.Name = record.Title;
                song.SongLength = record.SongLengthSpan;
                song.ReleaseInfo = record.Released;
                song.Comments = record.StilEntry;
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
            return csv.GetRecords<SidRecord>().ToDictionary(record => record.Filename);
        }

        private static Dictionary<string, SidRecord>  ParseSids(Dictionary<string, SidRecord> sids)
        {
            foreach (var sid in sids)
            {
                sid.Value.Title = sid.Value.Title.EnsureNotEmpty("Unknown Song");
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

        
    }
}