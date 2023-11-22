using CsvHelper;
using CsvHelper.Configuration;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Music
{
    public class MusicService : IMusicService
    {
        private readonly string _filePath;
        private readonly Dictionary<string, SidRecord> _recordsDictionary = new();

        public MusicService()
        {
            _filePath = GetSidFilePath(); 
            _recordsDictionary = ParseSids(ReadCsvFileToDictionary());
        }

        private static string GetSidFilePath()
        {
            var currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (currentDirectory is null) return string.Empty;

            var relativePath = @"Music\SIDlist_79.csv";

            return Path.Combine(currentDirectory, relativePath);
        }

        public SidRecord? Find(string filename)
        {
            _recordsDictionary.TryGetValue(filename, out var record);
            return record;
        }

        private Dictionary<string, SidRecord> ReadCsvFileToDictionary()
        {
            if (string.IsNullOrWhiteSpace(_filePath)) return _recordsDictionary;

            using var reader = new StreamReader(_filePath);
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