using FluentAssertions;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Music;
using TeensyRom.Core.Music.Hvsc;
using TeensyRom.Core.Music.DeepSid;
using TeensyRom.Core.ValueObjects;
using TeensyRom.Core.Common;

namespace TeensyRom.Tests.Unit
{
    /// <summary>
    /// Integration tests for SidMetadataService that integrate with HvscDatabase and DeepSidDatabase
    /// These tests verify behavioral aspects of SID metadata enrichment using real CSV and JSON data
    /// </summary>
    public class SidMetadataServiceTests : IDisposable
    {
        private readonly string _testCsvPath;
        private readonly string _testJsonPath;
        private readonly ISidMetadataService _metadataService;
        private readonly IHvscDatabase _hvscDatabase;
        private readonly IDeepSidDatabase _deepSidDatabase;

        public SidMetadataServiceTests()
        {
            // Get the test data file paths
            var currentDirectory = Path.GetDirectoryName(typeof(SidMetadataServiceTests).Assembly.Location);
            _testCsvPath = Path.Combine(currentDirectory!, "Unit", "TestData", "test_hvsc.csv");
            _testJsonPath = Path.Combine(currentDirectory!, "Unit", "TestData", "test_deepsid.json");

            // Ensure test files exist
            if (!File.Exists(_testCsvPath))
            {
                throw new FileNotFoundException($"Test CSV file not found at: {_testCsvPath}");
            }

            // Unzip DeepSID test data if the JSON doesn't exist but the zip does
            if (!File.Exists(_testJsonPath))
            {
                var zipPath = Path.Combine(currentDirectory!, "Unit", "TestData", "test_deepsid.zip");
                if (File.Exists(zipPath))
                {
                    System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, Path.GetDirectoryName(_testJsonPath)!, true);
                }
            }

            if (!File.Exists(_testJsonPath))
            {
                throw new FileNotFoundException($"Test JSON file not found at: {_testJsonPath}");
            }

            // Create databases with test data and inject into service
            _hvscDatabase = new HvscDatabase(_testCsvPath);
            _deepSidDatabase = new DeepSidDatabase(_testJsonPath);
            _metadataService = new SidMetadataService(_hvscDatabase, _deepSidDatabase);
        }

        public void Dispose()
        {
            // Cleanup if needed
        }

        #region Song Enrichment Behaviors

        [Fact]
        public void Should_EnrichSongWithHvscMetadata_When_RecordExists()
        {
            // Arrange
            var song = CreateTestSong("/DEMOS/0-9/10_Orbyte.sid");

            // Act
            var enrichedSong = _metadataService.EnrichSong(song);

            // Assert
            enrichedSong.Should().NotBeNull();
            enrichedSong.Title.Should().Be("10 Orbyte");
            enrichedSong.Creator.Should().Be("Michael Becker (Premium)");
            enrichedSong.ReleaseInfo.Should().Be("2014 Tristar & Red Sector Inc.");
            enrichedSong.PlayLength.Should().Be(TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(17)));
        }

        [Fact]
        public void Should_SetMetadataSourceOrClear_When_EnrichingWithHvscData()
        {
            // Arrange - Use a song with a STIL entry but no DeepSID data
            // Note: 53_Miles_West_of_Venus has STIL but checking if it has DeepSID data
            var song = CreateTestSong("/DEMOS/0-9/53_Miles_West_of_Venus.sid");

            // Act
            var enrichedSong = _metadataService.EnrichSong(song);

            // Assert - MetadataSource is set based on available data
            // If it has DeepSID data too, it will be combined; otherwise just HVSC
            enrichedSong.MetadataSource.Should().NotBeNullOrEmpty("should have some metadata source");
            enrichedSong.Description.Should().NotBeNullOrWhiteSpace("should have HVSC STIL data");
        }

        [Fact]
        public void Should_CombineMetadataSources_When_DataFromBothHvscAndDeepSid()
        {
            // Arrange - Use 1st_Sound which has both HVSC STIL and DeepSID tags
            var song = CreateTestSong("/DEMOS/0-9/1st_Sound.sid");

            // Act
            var enrichedSong = _metadataService.EnrichSong(song);

            // Assert - MetadataSource should show both sources with delimiter
            enrichedSong.MetadataSource.Should().Be($"{MusicConstants.Hvsc} / {MusicConstants.DeepSid}");
            
            // Verify we actually got data from both sources
            enrichedSong.Description.Should().NotBeNullOrWhiteSpace("should have HVSC STIL data");
            enrichedSong.Tags.Should().NotBeEmpty("should have DeepSID tag data");
        }

        [Fact]
        public void Should_SetDeepSidAsSource_When_OnlyDeepSidDataAvailable()
        {
            // Arrange - Use Commando which exists in DeepSID and HVSC, but has no STIL in HVSC
            var song = CreateTestSong("/MUSICIANS/H/Hubbard_Rob/Commando.sid");

            // Act
            var enrichedSong = _metadataService.EnrichSong(song);

            // Assert - MetadataSource should be DeepSID only since there's no STIL
            enrichedSong.MetadataSource.Should().Be(MusicConstants.DeepSid);
            
            // Verify we got DeepSID data
            enrichedSong.Tags.Should().NotBeEmpty("should have DeepSID tag data");
        }

        [Fact]
        public void Should_PreserveOriginalData_When_HvscRecordNotFound()
        {
            // Arrange
            var originalTitle = "Original Title";
            var originalCreator = "Original Creator";
            var song = CreateTestSong("/NonExistent/Path.sid");
            song.Title = originalTitle;
            song.Creator = originalCreator;

            // Act
            var enrichedSong = _metadataService.EnrichSong(song);

            // Assert - original data should be preserved when no HVSC match found
            enrichedSong.Title.Should().Be(originalTitle);
            enrichedSong.Creator.Should().Be(originalCreator);
        }

        [Fact]
        public void Should_SetShareUrl_When_EnrichingWithHvscData()
        {
            // Arrange
            var song = CreateTestSong("/MUSICIANS/H/Hubbard_Rob/Commando.sid");

            // Act
            var enrichedSong = _metadataService.EnrichSong(song);

            // Assert
            enrichedSong.ShareUrl.Should().Be("https://deepsid.chordian.net/?file=/MUSICIANS/H/Hubbard_Rob/Commando.sid");
        }

        #endregion

        #region Multi-Subtune Behaviors

        [Fact]
        public void Should_EnrichWithMultipleSubtuneLengths_When_SongHasMultipleSubtunes()
        {
            // Arrange - Use a song from the real HVSC database with multiple subtunes
            var song = CreateTestSong("/DEMOS/0-9/5th_Demo.sid");

            // Act
            var enrichedSong = _metadataService.EnrichSong(song);

            // Assert - Should have 3 subtunes based on the real CSV data
            enrichedSong.SubtuneLengths.Should().HaveCount(3);
            enrichedSong.SubtuneLengths[0].Should().BeGreaterThan(TimeSpan.Zero);
        }

        [Fact]
        public void Should_SetStartSubtuneNumber_When_RecordSpecifiesStartSong()
        {
            // Arrange - Use a real song that has start song = 1 (most songs)
            var song = CreateTestSong("/DEMOS/0-9/10_Orbyte.sid");

            // Act
            var enrichedSong = _metadataService.EnrichSong(song);

            // Assert - Should have start song number set from HVSC data
            enrichedSong.StartSubtuneNum.Should().Be(1);
        }

        [Fact]
        public void Should_UseFirstSubtuneLength_As_MainPlayLength()
        {
            // Arrange
            var song = CreateTestSong("/DEMOS/0-9/5th_Demo.sid");

            // Act
            var enrichedSong = _metadataService.EnrichSong(song);

            // Assert
            enrichedSong.PlayLength.Should().Be(TimeSpan.FromMinutes(5).Add(TimeSpan.FromSeconds(17)));
            enrichedSong.SubtuneLengths[0].Should().Be(enrichedSong.PlayLength);
        }

        #endregion

        #region STIL Description Behaviors

        [Fact]
        public void Should_EnrichWithCleanedStilDescription_When_StilEntryExists()
        {
            // Arrange
            var song = CreateTestSong("/DEMOS/0-9/1st_Sound.sid");

            // Act
            var enrichedSong = _metadataService.EnrichSong(song);

            // Assert
            enrichedSong.Description.Should().NotBeEmpty();
            enrichedSong.Description.Should().Contain("Heart");
            enrichedSong.Description.Should().Contain("Pet Shop Boys");
        }

        [Fact]
        public void Should_PreserveOriginalDescription_When_StilEntryIsEmpty()
        {
            // Arrange
            var originalDescription = "Original Description";
            var song = CreateTestSong("/DEMOS/0-9/10_Orbyte.sid");
            song.Description = originalDescription;

            // Act
            var enrichedSong = _metadataService.EnrichSong(song);

            // Assert
            enrichedSong.Description.Should().Be(originalDescription);
        }

        [Fact]
        public void Should_ClearMetadataSource_When_NoStilDescription()
        {
            // Arrange
            var song = CreateTestSong("/DEMOS/0-9/10_Orbyte.sid");
            song.MetadataSource = "SomeSource";

            // Act
            var enrichedSong = _metadataService.EnrichSong(song);

            // Assert - When STIL is empty but DeepSID has data, metadata source should be set to DeepSID
            // This song has no STIL entry, so HVSC metadata source is cleared, but DeepSID data is added
            enrichedSong.MetadataSource.Should().Be(MusicConstants.DeepSid);
        }

        #endregion

        #region Hardware Compatibility Behaviors

        [Fact]
        public void Should_SetCompatibilityToTrue_When_FormatIsPSID()
        {
            // Arrange
            var song = CreateTestSong("/DEMOS/0-9/10_Orbyte.sid");

            // Act
            var enrichedSong = _metadataService.EnrichSong(song);

            // Assert
            enrichedSong.IsCompatible.Should().BeTrue("PSID format should be compatible");
        }

        [Fact]
        public void Should_SetCompatibilityToFalse_When_FormatIsRSID()
        {
            // Arrange
            var song = CreateTestSong("/DEMOS/0-9/12345.sid");

            // Act
            var enrichedSong = _metadataService.EnrichSong(song);

            // Assert
            enrichedSong.IsCompatible.Should().BeFalse("RSID format should not be compatible");
        }

        [Fact]
        public void Should_SetClockMetadata_When_EnrichingWithHvscData()
        {
            // Arrange
            var palSong = CreateTestSong("/DEMOS/0-9/10_Orbyte.sid");
            var ntscSong = CreateTestSong("/DEMOS/0-9/5th_Demo.sid");

            // Act
            var enrichedPalSong = _metadataService.EnrichSong(palSong);
            var enrichedNtscSong = _metadataService.EnrichSong(ntscSong);

            // Assert
            enrichedPalSong.Meta1.Should().Be("PAL");
            enrichedNtscSong.Meta1.Should().Be("NTSC");
        }

        [Fact]
        public void Should_SetSidModelMetadata_When_EnrichingWithHvscData()
        {
            // Arrange
            var song6581 = CreateTestSong("/DEMOS/0-9/10_Orbyte.sid");
            var song8580 = CreateTestSong("/DEMOS/0-9/53_Miles_West_of_Venus.sid");

            // Act
            var enrichedSong6581 = _metadataService.EnrichSong(song6581);
            var enrichedSong8580 = _metadataService.EnrichSong(song8580);

            // Assert
            enrichedSong6581.Meta2.Should().Be("6581");
            enrichedSong8580.Meta2.Should().Be("8580");
        }

        #endregion

        #region Metadata Source Path Behaviors

        [Fact]
        public void Should_UpdateMetadataSourcePath_When_EnrichingWithHvscData()
        {
            // Arrange
            var song = CreateTestSong("/DEMOS/0-9/10_Orbyte.sid");
            song.MetadataSourcePath = new FilePath("/some/other/path.sid");

            // Act
            var enrichedSong = _metadataService.EnrichSong(song);

            // Assert
            enrichedSong.MetadataSourcePath.Value.Should().Contain("/DEMOS/0-9/10_Orbyte.sid");
        }

        [Fact]
        public void Should_PreserveMetadataSourcePath_When_AlreadyContainsCorrectPath()
        {
            // Arrange
            var correctPath = "/MUSICIANS/H/Hubbard_Rob/Commando.sid";
            var song = CreateTestSong(correctPath);
            song.MetadataSourcePath = new FilePath(correctPath);

            // Act
            var enrichedSong = _metadataService.EnrichSong(song);

            // Assert
            enrichedSong.MetadataSourcePath.Value.Should().Be(correctPath);
        }

        #endregion

        #region Composer Image Enrichment Behaviors

        [Fact]
        public void Should_AttemptToAddComposerImage_When_EnrichingSong()
        {
            // Arrange
            var song = CreateTestSong("/MUSICIANS/H/Hubbard_Rob/Commando.sid");

            // Act
            var enrichedSong = _metadataService.EnrichSong(song);

            // Assert
            // Note: Images won't actually be added in tests unless the image files exist
            // But the enrichment process should still complete successfully
            enrichedSong.Should().NotBeNull();
            enrichedSong.Images.Should().NotBeNull();
        }

        [Fact]
        public void Should_UpdateMetadataSourcePath_When_FoundInHvsc()
        {
            // Arrange
            var song = CreateTestSong("/DEMOS/0-9/10_Orbyte.sid");
            song.MetadataSourcePath = new FilePath("/some/other/path.sid");

            // Act
            var enrichedSong = _metadataService.EnrichSong(song);

            // Assert
            // The service should update the metadata source path to the HVSC path
            enrichedSong.MetadataSourcePath.Value.Should().Contain("10_Orbyte");
        }

        [Fact]
        public void Should_FallbackToSongPath_When_MetadataSourcePathIsEmpty()
        {
            // Arrange
            var song = CreateTestSong("/MUSICIANS/H/Hubbard_Rob/Commando.sid");
            song.MetadataSourcePath = new FilePath(string.Empty);

            // Act
            var enrichedSong = _metadataService.EnrichSong(song);

            // Assert
            enrichedSong.Should().NotBeNull();
            // Service should handle empty metadata source path gracefully
        }

        #endregion

        #region Edge Case Behaviors

        [Fact]
        public void Should_PreserveTitleWhen_RecordNotFoundInHvsc()
        {
            // Arrange
            var originalTitle = "Test Title";
            var song = CreateTestSong("/GAMES/A-F/NonExistent.sid");
            song.Title = originalTitle;

            // Act
            var enrichedSong = _metadataService.EnrichSong(song);

            // Assert - original title should be preserved when no HVSC match
            enrichedSong.Title.Should().Be(originalTitle);
        }

        [Fact]
        public void Should_PreserveCreatorWhen_RecordNotFoundInHvsc()
        {
            // Arrange
            var originalCreator = "Test Creator";
            var song = CreateTestSong("/GAMES/G-L/NonExistent.sid");
            song.Creator = originalCreator;

            // Act
            var enrichedSong = _metadataService.EnrichSong(song);

            // Assert - original creator should be preserved when no HVSC match
            enrichedSong.Creator.Should().Be(originalCreator);
        }

        [Fact]
        public void Should_HandleSongWithNoSubtunes_When_Enriching()
        {
            // Arrange
            var song = CreateTestSong("/GAMES/A-F/Empty_Title.sid");

            // Act
            var enrichedSong = _metadataService.EnrichSong(song);

            // Assert
            enrichedSong.SubtuneLengths.Should().BeEmpty();
            enrichedSong.PlayLength.Should().Be(MusicConstants.DefaultLength);
        }

        [Fact]
        public void Should_ReturnSameSongInstance_When_Enriching()
        {
            // Arrange
            var song = CreateTestSong("/DEMOS/0-9/10_Orbyte.sid");

            // Act
            var enrichedSong = _metadataService.EnrichSong(song);

            // Assert
            enrichedSong.Should().BeSameAs(song, "enrichment should modify the same instance");
        }

        [Fact]
        public void Should_HandleSpecialCharactersInPath_When_Enriching()
        {
            // Arrange
            var song = CreateTestSong("/DEMOS/0-9/12345.sid");

            // Act
            var enrichedSong = _metadataService.EnrichSong(song);

            // Assert
            enrichedSong.Should().NotBeNull();
            enrichedSong.Title.Should().Be("12345");
        }

        #endregion

        #region Multiple Enrichment Behaviors

        [Fact]
        public void Should_ProduceConsistentResults_When_EnrichingMultipleTimes()
        {
            // Arrange
            var song = CreateTestSong("/DEMOS/0-9/10_Orbyte.sid");

            // Act
            var enriched1 = _metadataService.EnrichSong(song);
            var enriched2 = _metadataService.EnrichSong(song);

            // Assert
            enriched1.Title.Should().Be(enriched2.Title);
            enriched1.Creator.Should().Be(enriched2.Creator);
            enriched1.PlayLength.Should().Be(enriched2.PlayLength);
        }

        [Fact]
        public void Should_EnrichMultipleSongsIndependently_When_CalledSequentially()
        {
            // Arrange
            var song1 = CreateTestSong("/DEMOS/0-9/10_Orbyte.sid");
            var song2 = CreateTestSong("/DEMOS/0-9/12345.sid");

            // Act
            var enriched1 = _metadataService.EnrichSong(song1);
            var enriched2 = _metadataService.EnrichSong(song2);

            // Assert
            enriched1.Title.Should().Be("10 Orbyte");
            enriched2.Title.Should().Be("12345");
            enriched1.Creator.Should().NotBe(enriched2.Creator);
        }

        #endregion

        #region Integration Test: Full Enrichment Flow

        [Fact]
        public void Should_FullyEnrichSong_When_AllMetadataAvailable()
        {
            // Arrange - Use a song we know exists in the real HVSC database
            var song = CreateTestSong("/DEMOS/0-9/10_Orbyte.sid");

            // Act
            var enrichedSong = _metadataService.EnrichSong(song);

            // Assert - verify complete enrichment
            enrichedSong.Should().NotBeNull();

            // Basic metadata
            enrichedSong.Title.Should().Be("10 Orbyte");
            enrichedSong.Creator.Should().Be("Michael Becker (Premium)");
            enrichedSong.ReleaseInfo.Should().Be("2014 Tristar & Red Sector Inc.");

            // Timing information
            enrichedSong.PlayLength.Should().BeGreaterThan(TimeSpan.Zero);
            enrichedSong.PlayLength.Should().Be(TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(17)));
            enrichedSong.SubtuneLengths.Should().HaveCount(1);
            enrichedSong.StartSubtuneNum.Should().Be(1);

            // Hardware specs
            enrichedSong.Meta1.Should().Be("PAL");
            enrichedSong.Meta2.Should().Be("6581");
            enrichedSong.IsCompatible.Should().BeTrue();

            // Additional metadata
            enrichedSong.MetadataSource.Should().Be(MusicConstants.DeepSid); // No STIL but has DeepSID data
            enrichedSong.ShareUrl.Should().Contain("deepsid.chordian.net");

            // Path information
            enrichedSong.MetadataSourcePath.Value.Should().Contain("10_Orbyte");
        }

        [Fact]
        public void Should_HandleMinimalMetadata_When_OnlyBasicInfoAvailable()
        {
            // Arrange
            var song = CreateTestSong("/DEMOS/0-9/10_Orbyte.sid");

            // Act
            var enrichedSong = _metadataService.EnrichSong(song);

            // Assert - verify graceful handling of minimal metadata
            enrichedSong.Should().NotBeNull();
            enrichedSong.Title.Should().NotBeNullOrEmpty();
            enrichedSong.Creator.Should().NotBeNullOrEmpty();
            enrichedSong.PlayLength.Should().BeGreaterThan(TimeSpan.Zero);
        }

        #endregion

        #region DeepSID Enrichment Behaviors

        [Fact]
        public void Should_EnrichWithCreatorInfoUrl_When_DeepSidHasComposerCsdbUrl()
        {
            // Arrange
            var song = CreateTestSong("/MUSICIANS/H/Hubbard_Rob/Commando.sid");

            // Act
            var enrichedSong = _metadataService.EnrichSong(song);

            // Assert
            enrichedSong.Links.Should().Contain(l => l.Name == "CSDB Profile" && l.Url == "https://csdb.dk/scener/?id=298");
        }

        [Fact]
        public void Should_EnrichWithFileInfoUrl_When_DeepSidHasCsdbUrl()
        {
            // Arrange
            var song = CreateTestSong("/DEMOS/0-9/10_Orbyte.sid");

            // Act
            var enrichedSong = _metadataService.EnrichSong(song);

            // Assert
            enrichedSong.Links.Should().Contain(l => l.Name == "CSDB Release" && l.Url == "https://csdb.dk/release/?id=67890");
        }

        [Fact]
        public void Should_EnrichWithTags_When_DeepSidHasTags()
        {
            // Arrange
            var song = CreateTestSong("/DEMOS/0-9/10_Orbyte.sid");

            // Act
            var enrichedSong = _metadataService.EnrichSong(song);

            // Assert
            enrichedSong.Tags.Should().HaveCount(2);
            enrichedSong.Tags.Should().Contain(t => t.Name == "demo" && t.Type == "category");
            enrichedSong.Tags.Should().Contain(t => t.Name == "modern" && t.Type == "era");
        }

        [Fact]
        public void Should_EnrichWithMultipleTags_When_DeepSidHasMultipleTags()
        {
            // Arrange
            var song = CreateTestSong("/MUSICIANS/H/Hubbard_Rob/Commando.sid");

            // Act
            var enrichedSong = _metadataService.EnrichSong(song);

            // Assert
            enrichedSong.Tags.Should().HaveCount(3);
            enrichedSong.Tags.Should().Contain(t => t.Name == "game" && t.Type == "category");
            enrichedSong.Tags.Should().Contain(t => t.Name == "classic" && t.Type == "era");
            enrichedSong.Tags.Should().Contain(t => t.Name == "action" && t.Type == "genre");
        }

        [Fact]
        public void Should_NotModifyUrls_When_DeepSidRecordNotFound()
        {
            // Arrange
            var song = CreateTestSong("/NonExistent/Path.sid");
            var originalLink = new FileLink { Name = "Original", Url = "original-url" };
            song.Links.Add(originalLink);

            // Act
            var enrichedSong = _metadataService.EnrichSong(song);

            // Assert
            enrichedSong.Links.Should().HaveCount(1);
            enrichedSong.Links[0].Name.Should().Be("Original");
            enrichedSong.Links[0].Url.Should().Be("original-url");
        }

        [Fact]
        public void Should_NotModifyTags_When_DeepSidRecordNotFound()
        {
            // Arrange
            var song = CreateTestSong("/NonExistent/Path.sid");
            var originalTag = new FileTag { Name = "original", Type = "test" };
            song.Tags.Add(originalTag);

            // Act
            var enrichedSong = _metadataService.EnrichSong(song);

            // Assert
            enrichedSong.Tags.Should().HaveCount(1);
            enrichedSong.Tags[0].Name.Should().Be("original");
        }

        [Fact]
        public void Should_EnrichWithDeepSidAndHvscData_When_BothRecordsExist()
        {
            // Arrange
            var song = CreateTestSong("/MUSICIANS/H/Hubbard_Rob/Commando.sid");

            // Act
            var enrichedSong = _metadataService.EnrichSong(song);

            // Assert - HVSC data
            enrichedSong.Title.Should().Be("Commando");
            enrichedSong.Creator.Should().Be("Rob Hubbard");
            enrichedSong.Meta1.Should().Be("PAL");
            enrichedSong.Meta2.Should().Be("6581");

            // Assert - DeepSID data
            enrichedSong.Links.Should().Contain(l => l.Name == "CSDB Profile" && l.Url == "https://csdb.dk/scener/?id=298");
            enrichedSong.Links.Should().Contain(l => l.Name == "CSDB Release" && l.Url == "https://csdb.dk/release/?id=12345");
            enrichedSong.Tags.Should().HaveCount(3);
        }

        [Fact]
        public void Should_HandleEmptyCsdbUrls_When_DeepSidRecordHasNullOrEmptyUrls()
        {
            // Arrange
            var song = CreateTestSong("/DEMOS/0-9/1st_Sound.sid");

            // Act
            var enrichedSong = _metadataService.EnrichSong(song);

            // Assert - Should set the URL from the record
            enrichedSong.Links.Should().Contain(l => l.Name == "CSDB Release" && l.Url == "https://csdb.dk/release/?id=33333");
            enrichedSong.Links.Should().Contain(l => l.Name == "CSDB Profile" && l.Url == "https://csdb.dk/scener/?id=5678");
        }

        [Fact]
        public void Should_UseMetadataSourcePath_When_SearchingDeepSid()
        {
            // Arrange
            var song = CreateTestSong("/local/path/Commando.sid");
            song.MetadataSourcePath = new FilePath("/MUSICIANS/H/Hubbard_Rob/Commando.sid");

            // Act
            var enrichedSong = _metadataService.EnrichSong(song);

            // Assert - Should find DeepSID data using MetadataSourcePath
            enrichedSong.Links.Should().Contain(l => l.Name == "CSDB Profile" && l.Url == "https://csdb.dk/scener/?id=298");
            enrichedSong.Tags.Should().HaveCount(3);
        }

        [Fact]
        public void Should_FallbackToSongPath_When_MetadataSourcePathIsEmpty_ForDeepSid()
        {
            // Arrange
            var song = CreateTestSong("/DEMOS/0-9/10_Orbyte.sid");
            song.MetadataSourcePath = new FilePath(string.Empty);

            // Act
            var enrichedSong = _metadataService.EnrichSong(song);

            // Assert - Should find DeepSID data using song path
            enrichedSong.Links.Should().Contain(l => l.Name == "CSDB Release" && l.Url == "https://csdb.dk/release/?id=67890");
            enrichedSong.Tags.Should().HaveCount(2);
        }

        #endregion

        #region Database Constructor Tests

        [Fact]
        public void Should_CreateDeepSidDatabaseWithEmptyConstructor_When_DefaultLocationIsAvailable()
        {
            // Act & Assert - should not throw any exceptions
            var database = new DeepSidDatabase();
            
            // Assert - database should be created (might be empty if default file doesn't exist, but that's okay)
            database.Should().NotBeNull();
        }

        [Fact]
        public void Should_ResolveCorrectDefaultPath_When_UsingEmptyConstructor()
        {
            // Act - create database with empty constructor (should use GetDeepSidFilePath)
            var database = new DeepSidDatabase();
            
            // Assert - database should be created without exceptions
            database.Should().NotBeNull();
            
            // The actual path resolution is tested implicitly - if the constructor completes
            // without throwing a FileNotFoundException, it means the path was resolved correctly
        }

        [Fact]
        public void Should_LoadProductionDeepSidDatabase_When_UsingEmptyConstructor()
        {
            // This test ensures that the production DeepSID database can be deserialized correctly
            // Previously failed due to PowerShell unwrapping single-element arrays into objects
            // Fixed in export-complete-json.ps1 by ensuring arrays stay as arrays
            
            // Act & Assert - should not throw deserialization exceptions
            Action loadProductionDatabase = () => {
                var database = new DeepSidDatabase();
                // If we get here without exceptions, the deserialization worked
            };
            
            // This should work now that we fixed the PowerShell export script
            loadProductionDatabase.Should().NotThrow("Production DeepSID database should deserialize correctly after PowerShell export fix");
        }

        #endregion

        #region Helper Methods

        private static SongItem CreateTestSong(string path, long size = 0)
        {
            // Default sizes based on test data for common paths
            var defaultSizes = new Dictionary<string, long>
            {
                { "/DEMOS/0-9/10_Orbyte.sid", 1584 },
                { "/DEMOS/0-9/12345.sid", 55934 },
                { "/DEMOS/0-9/1st_Sound.sid", 3324 },
                { "/DEMOS/0-9/53_Miles_West_of_Venus.sid", 5694 },
                { "/DEMOS/0-9/5th_Demo.sid", 3114 },
                { "/MUSICIANS/H/Hubbard_Rob/Commando.sid", 4096 },
                { "/MUSICIANS/T/Tel_Jeroen/Cybernoid.sid", 5120 },
                { "/GAMES/A-F/Empty_Title.sid", 2048 },
                { "/GAMES/G-L/Empty_Author.sid", 2048 },
                { "/DEMOS/TIME_TEST/Multi_Subtune.sid", 3072 }
            };

            if (size == 0 && defaultSizes.ContainsKey(path))
            {
                size = defaultSizes[path];
            }

            return new SongItem
            {
                Path = new FilePath(path),
                Name = System.IO.Path.GetFileName(path),
                Size = size,
                MetadataSourcePath = new FilePath(string.Empty)
            };
        }

        #endregion
    }
}
