using System.Reflection;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Music.Hvsc;
using TeensyRom.Core.Music.Sid;
using TeensyRom.Core.Music.DeepSid;
using TeensyRom.Core.ValueObjects;

namespace TeensyRom.Core.Music
{
    public interface ISidMetadataService
    {
        SongItem EnrichSong(SongItem song);
    }

    public class SidMetadataService : ISidMetadataService
    {
        private readonly IHvscDatabase _hvscDatabase;
        private readonly IDeepSidDatabase _deepSidDatabase;

        public SidMetadataService(IHvscDatabase hvscDatabase, IDeepSidDatabase deepSidDatabase)
        {
            _hvscDatabase = hvscDatabase;
            _deepSidDatabase = deepSidDatabase;
        }

        public SongItem EnrichSong(SongItem song)
        {
            var sidRecord = _hvscDatabase.GetRecord(song.Id);

            if (sidRecord is not null)
            {
                EnrichWithHvscMetadata(song, sidRecord);
            }

            EnrichWithDeepSidData(song);
            EnrichWithDeepSidComposerImage(song);

            return song;
        }

        private void EnrichWithHvscMetadata(SongItem song, SidRecord sidRecord)
        {
            var missingSTIL = string.IsNullOrWhiteSpace(sidRecord.StilEntry);

            song.Description = missingSTIL ? song.Description : sidRecord.StilEntry;
            
            // Add HVSC to metadata source if we have STIL data
            if (!missingSTIL)
            {
                AddMetadataSource(song, MusicConstants.Hvsc);
            }
            else
            {
                // Clear metadata source if no STIL (will be set by other sources if they have data)
                song.MetadataSource = string.Empty;
            }
            
            song.Creator = sidRecord.Author;
            song.Title = sidRecord.Title;
            song.PlayLength = sidRecord.SongLengthSpan;
            song.StartSubtuneNum = sidRecord.StartSong;
            song.SubtuneLengths = sidRecord.SubTuneSongLengths.ToList();
            song.ReleaseInfo = sidRecord.Released;
            song.Meta1 = sidRecord.Clock;
            song.Meta2 = sidRecord.SidModel;
            song.IsCompatible = sidRecord.Format != MusicConstants.RSID;

            song.MetadataSourcePath = song.MetadataSourcePath.Value.Contains(sidRecord.Filepath)
                ? song.MetadataSourcePath
                : new FilePath(sidRecord.Filepath);

            song.ShareUrl = $"https://deepsid.chordian.net/?file={sidRecord.Filepath}";
        }

        private void EnrichWithDeepSidData(SongItem song)
        {
            var songPath = song.MetadataSourcePath.IsEmpty ? song.Path.Value : song.MetadataSourcePath.Value;
            var deepSidRecord = _deepSidDatabase.SearchByPath(songPath);

            if (deepSidRecord is null) return;

            var hasData = false;

            // Convert CSDB URLs to FileLinks
            if (!string.IsNullOrWhiteSpace(deepSidRecord.CsdbUrl))
            {
                song.Links.Add(new Entities.Storage.FileLink
                {
                    Name = "CSDB Release",
                    Url = deepSidRecord.CsdbUrl
                });
                hasData = true;
            }

            if (deepSidRecord.Composer?.Csdb?.Url is not null)
            {
                song.Links.Add(new Entities.Storage.FileLink
                {
                    Name = "CSDB Profile",
                    Url = deepSidRecord.Composer.Csdb.Url
                });
                hasData = true;
            }

            // Add composer links from DeepSID
            if (deepSidRecord.Composer?.Links?.Count > 0)
            {
                foreach (var composerLink in deepSidRecord.Composer.Links)
                {
                    song.Links.Add(new Entities.Storage.FileLink
                    {
                        Name = composerLink.Name,
                        Url = composerLink.Url
                    });
                }
                hasData = true;
            }

            // Add YouTube videos
            if (deepSidRecord.YouTubeVideos?.Count > 0)
            {
                song.YouTubeVideos = deepSidRecord.YouTubeVideos
                    .Select(v => new Entities.Storage.YouTubeVideo 
                    { 
                        VideoId = v.VideoId,
                        Url = v.Url,
                        Channel = v.Channel,
                        Subtune = v.Subtune
                    })
                    .ToList();
                hasData = true;
            }

            // Add competitions
            if (deepSidRecord.Competitions?.Count > 0)
            {
                song.Competitions = deepSidRecord.Competitions
                    .Select(c => new Entities.Storage.Competition 
                    { 
                        Name = c.Name,
                        Place = c.Place
                    })
                    .ToList();
                hasData = true;
            }

            // Add rating data
            if (deepSidRecord.AvgRating.HasValue || deepSidRecord.RatingCount > 0)
            {
                song.AvgRating = (decimal?)deepSidRecord.AvgRating;
                song.RatingCount = deepSidRecord.RatingCount;
                hasData = true;
            }

            // Enrich with tags
            if (deepSidRecord.Tags.Count > 0)
            {
                song.Tags = deepSidRecord.Tags
                    .Select(t => new Entities.Storage.FileTag
                    {
                        Name = t.Name,
                        Type = t.Type
                    })
                    .ToList();
                hasData = true;
            }

            // Add DeepSID to metadata source if we got any data
            if (hasData)
            {
                AddMetadataSource(song, MusicConstants.DeepSid);
            }
        }

        /// <summary>
        /// Adds a metadata source to the song's MetadataSource field.
        /// If MetadataSource is already populated, appends with " / " delimiter.
        /// </summary>
        private static void AddMetadataSource(SongItem song, string source)
        {
            if (string.IsNullOrWhiteSpace(song.MetadataSource))
            {
                song.MetadataSource = source;
            }
            else
            {
                song.MetadataSource = $"{song.MetadataSource} / {source}";
            }
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
        private void EnrichWithDeepSidComposerImage(SongItem song)
        {
            var currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var songPath = song.MetadataSourcePath.IsEmpty ? song.Path : song.MetadataSourcePath;

            var remainingPathSegments = songPath.Value.GetRemainingPathSegments(MusicConstants.Hvsc_Musician_Base_Remote_Path);

            var hsvcImageName = $"{Path.Combine(currentDirectory!, MusicConstants.Musician_Image_Local_Path)}musicians";

            foreach (var segment in remainingPathSegments)
            {
                hsvcImageName = $"{hsvcImageName}_{segment}";
            }
            hsvcImageName = $"{hsvcImageName}.jpg";

            if (File.Exists(hsvcImageName))
            {
                var fileName = Path.GetFileName(hsvcImageName);
                song.Images.Add(new ViewableItemImage
                {
                    FileName = fileName,
                    Path = hsvcImageName,
                    BaseAssetPath = "/" + MusicConstants.Musician_Image_Local_Path.Replace('\\', '/') + "/" + fileName,
                    Source = MusicConstants.DeepSid
                });
            }
        }
    }
}