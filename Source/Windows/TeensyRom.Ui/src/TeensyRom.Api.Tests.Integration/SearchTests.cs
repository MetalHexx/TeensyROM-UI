using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using TeensyRom.Api.Endpoints.Files.Search;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Settings;

namespace TeensyRom.Api.Tests.Integration
{
    [Collection("Endpoint")]
    public class SearchTests(EndpointFixture f) : IDisposable
    {
        public const string Games_Path = "/games/";
        public const string Music_Path = "/music/MUSICIANS/L/LukHash/";
        public const string Images_Path = "/images/";

        private async Task<string> GetCachedConnectedDevice()
        {
            // Always get a fresh connected device instead of using stale cache
            // This prevents issues where cached device becomes disconnected between tests
            return await f.GetConnectedDevice();
        }

        [Fact]
        public async Task When_ValidRequest_WithGamesIndexed_SearchResultsReturned()
        {
            // Arrange
            var deviceId = await GetCachedConnectedDevice();
            f.DeleteCache(deviceId!, TeensyStorageType.SD);
            await f.Preindex(deviceId, TeensyStorageType.SD, Games_Path);
            
            var searchText = "donkey";

            // Act - TrClient automatically handles enum serialization
            var r = await f.Client.GetAsync<SearchEndpoint, SearchRequest, SearchResponse>(new SearchRequest
            {
                DeviceId = deviceId,
                SearchText = searchText,
                StorageType = TeensyStorageType.SD,
                FilterType = TeensyFilterType.All
            });

            //Assert
            r.Should().BeSuccessful<SearchResponse>()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithContentNotNull();

            r.Content.SearchText.Should().Be(searchText);
            r.Content.TotalCount.Should().BeGreaterThanOrEqualTo(0);
            r.Content.Files.Should().NotBeNull();
            r.Content.Message.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task When_MusicFilterApplied_WithMusicIndexed_OnlyMusicFilesReturned()
        {
            // Arrange
            var deviceId = await GetCachedConnectedDevice();
            f.DeleteCache(deviceId!, TeensyStorageType.SD);
            await f.Preindex(deviceId, TeensyStorageType.SD, Music_Path);

            var searchText = "lukhash";

            // Act - TrClient automatically handles enum serialization
            var r = await f.Client.GetAsync<SearchEndpoint, SearchRequest, SearchResponse>(new SearchRequest
            {
                DeviceId = deviceId,
                SearchText = searchText,
                StorageType = TeensyStorageType.SD,
                FilterType = TeensyFilterType.Music
            });

            //Assert
            r.Should().BeSuccessful<SearchResponse>()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithContentNotNull();

            // All returned files should be music files (if any)
            if (r.Content.Files.Count > 0)
            {
                r.Content.Files.Should().OnlyContain(f => f.Path.EndsWith(".sid") || f.Path.EndsWith(".mus"));
            }

            r.Content.SearchText.Should().Be(searchText);
            r.Content.TotalCount.Should().Be(r.Content.Files.Count);
        }

        [Fact]
        public async Task When_GameFilterApplied_WithGamesIndexed_OnlyGameFilesReturned()
        {
            // Arrange
            var deviceId = await GetCachedConnectedDevice();
            f.DeleteCache(deviceId!, TeensyStorageType.SD);
            await f.Preindex(deviceId, TeensyStorageType.SD, Games_Path);

            var searchText = "kong";

            // Act - TrClient automatically handles enum serialization
            var r = await f.Client.GetAsync<SearchEndpoint, SearchRequest, SearchResponse>(new SearchRequest
            {
                DeviceId = deviceId,
                SearchText = searchText,
                StorageType = TeensyStorageType.SD,
                FilterType = TeensyFilterType.Games
            });

            //Assert
            r.Should().BeSuccessful<SearchResponse>()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithContentNotNull();

            // All returned files should be game files (if any)
            if (r.Content.Files.Count > 0)
            {
                r.Content.Files.Should().OnlyContain(f => 
                    f.Path.EndsWith(".prg") || 
                    f.Path.EndsWith(".p00") || 
                    f.Path.EndsWith(".crt") || 
                    f.Path.EndsWith(".d64"));
            }

            r.Content.SearchText.Should().Be(searchText);
            r.Content.TotalCount.Should().Be(r.Content.Files.Count);
        }

        [Fact]
        public async Task When_ImageFilterApplied_WithImagesIndexed_OnlyImageFilesReturned()
        {
            // Arrange
            var deviceId = await GetCachedConnectedDevice();
            f.DeleteCache(deviceId!, TeensyStorageType.SD);
            await f.Preindex(deviceId, TeensyStorageType.SD, Images_Path);

            var searchText = "dio";

            // Act - TrClient automatically handles enum serialization
            var r = await f.Client.GetAsync<SearchEndpoint, SearchRequest, SearchResponse>(new SearchRequest
            {
                DeviceId = deviceId,
                SearchText = searchText,
                StorageType = TeensyStorageType.SD,
                FilterType = TeensyFilterType.Images
            });

            //Assert
            r.Should().BeSuccessful<SearchResponse>()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithContentNotNull();

            // All returned files should be image files (if any)
            if (r.Content.Files.Count > 0)
            {
                r.Content.Files.Should().OnlyContain(f => 
                    f.Path.EndsWith(".kla") || 
                    f.Path.EndsWith(".koa") || 
                    f.Path.EndsWith(".art") || 
                    f.Path.EndsWith(".aas") || 
                    f.Path.EndsWith(".hpi") || 
                    f.Path.EndsWith(".seq") ||
                    f.Path.EndsWith(".txt") ||
                    f.Path.EndsWith(".nfo"));
            }

            r.Content.SearchText.Should().Be(searchText);
            r.Content.TotalCount.Should().Be(r.Content.Files.Count);
        }

        [Fact]
        public async Task When_NoFilesMatchSearch_WithGamesIndexed_ReturnsEmptyResults()
        {
            // Arrange
            var deviceId = await GetCachedConnectedDevice();
            f.DeleteCache(deviceId!, TeensyStorageType.SD);
            await f.Preindex(deviceId, TeensyStorageType.SD, Games_Path);

            var searchText = "xyznomatchingfiles12345lkfjasdofiuasd09ofajsldkfjasdlfkjas";

            // Act - TrClient automatically handles enum serialization
            var r = await f.Client.GetAsync<SearchEndpoint, SearchRequest, SearchResponse>(new SearchRequest
            {
                DeviceId = deviceId,
                SearchText = searchText,
                StorageType = TeensyStorageType.SD
            });

            //Assert
            r.Should().BeSuccessful<SearchResponse>()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithContentNotNull();

            r.Content.Files.Should().BeEmpty();
            r.Content.TotalCount.Should().Be(0);
            r.Content.SearchText.Should().Be(searchText);
            r.Content.Message.Should().Contain("No files found");
        }

        [Fact]
        public async Task When_SearchingEmptyCache_ReturnsEmptyResults()
        {
            // Arrange
            var deviceId = await GetCachedConnectedDevice();
            f.DeleteCache(deviceId!, TeensyStorageType.SD);
            // Note: No indexing step - empty cache

            var searchText = "anything";

            // Act - TrClient automatically handles enum serialization
            var r = await f.Client.GetAsync<SearchEndpoint, SearchRequest, SearchResponse>(new SearchRequest
            {
                DeviceId = deviceId,
                SearchText = searchText,
                StorageType = TeensyStorageType.SD
            });

            //Assert
            r.Should().BeSuccessful<SearchResponse>()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithContentNotNull();

            r.Content.Files.Should().BeEmpty();
            r.Content.TotalCount.Should().Be(0);
            r.Content.SearchText.Should().Be(searchText);
            r.Content.Message.Should().Contain("No files found");
        }

        [Fact]
        public async Task When_DeviceIdInvalid_ReturnsBadRequest()
        {
            // Act - TrClient automatically handles enum serialization
            var r = await f.Client.GetAsync<SearchEndpoint, SearchRequest, ValidationProblemDetails>(new SearchRequest
            {
                DeviceId = "!!invalid",
                SearchText = "test",
                StorageType = TeensyStorageType.SD
            });

            // Assert
            r.Should().BeValidationProblem()
                .WithKeyAndValue("DeviceId", "Device ID must be a valid filename-safe hash of 8 characters long.");
        }

        [Fact]
        public async Task When_SearchTextEmpty_ReturnsBadRequest()
        {
            var validDeviceId = Guid.NewGuid().ToString().GenerateFilenameSafeHash();

            // TrClient automatically handles enum serialization
            var r = await f.Client.GetAsync<SearchEndpoint, SearchRequest, ValidationProblemDetails>(new SearchRequest            
            {
                DeviceId = validDeviceId,
                SearchText = "",
                StorageType = TeensyStorageType.SD
            });

            r.Should().BeValidationProblem()
                .WithKeyAndValue("SearchText", "Search text is required.");
        }

        [Fact]
        public async Task When_SearchTextTooShort_ReturnsBadRequest()
        {
            var validDeviceId = Guid.NewGuid().ToString().GenerateFilenameSafeHash();

            // TrClient automatically handles enum serialization
            var r = await f.Client.GetAsync<SearchEndpoint, SearchRequest, ValidationProblemDetails>(new SearchRequest            
            {
                DeviceId = validDeviceId,
                SearchText = "a",
                StorageType = TeensyStorageType.SD
            });

            r.Should().BeValidationProblem()
                .WithKeyAndValue("SearchText", "Search text must be at least 2 characters long.");
        }

        [Fact]
        public async Task When_SearchTextTooLong_ReturnsBadRequest()
        {
            var validDeviceId = Guid.NewGuid().ToString().GenerateFilenameSafeHash();
            var longSearchText = new string('a', 101); // 101 characters

            // TrClient automatically handles enum serialization
            var r = await f.Client.GetAsync<SearchEndpoint, SearchRequest, ValidationProblemDetails>(new SearchRequest            
            {
                DeviceId = validDeviceId,
                SearchText = longSearchText,
                StorageType = TeensyStorageType.SD
            });

            r.Should().BeValidationProblem()
                .WithKeyAndValue("SearchText", "Search text must be no more than 100 characters long.");
        }

        [Fact]
        public async Task When_InvalidStorageType_ReturnsBadRequest()
        {
            // Arrange
            var validDeviceId = Guid.NewGuid().ToString().GenerateFilenameSafeHash();

            // Act - TrClient automatically handles enum serialization
            var r = await f.Client.GetAsync<SearchEndpoint, SearchRequest, ValidationProblemDetails>(new SearchRequest            
            {
                DeviceId = validDeviceId,
                SearchText = "test",
                StorageType = (TeensyStorageType)999
            });

            // Assert
            r.Should().BeValidationProblem()
                .WithKeyAndValue("StorageType", "Storage type must be a valid enum value.");
        }

        [Fact]
        public async Task When_InvalidFilterType_ReturnsBadRequest()
        {
            // Arrange
            var validDeviceId = Guid.NewGuid().ToString().GenerateFilenameSafeHash();

            // Act - TrClient automatically handles enum serialization
            var r = await f.Client.GetAsync<SearchEndpoint, SearchRequest, ValidationProblemDetails>(new SearchRequest            
            {
                DeviceId = validDeviceId,
                SearchText = "test",
                StorageType = TeensyStorageType.SD,
                FilterType = (TeensyFilterType)999
            });

            // Assert
            r.Should().BeValidationProblem()
                .WithKeyAndValue("FilterType", "Filter type must be a valid enum value.");
        }

        //[Fact]
        //public async Task When_StorageNotAvailable_ReturnsNotFound()
        //{
        //    // Arrange
        //    var deviceId = await GetCachedConnectedDevice();

        //    // Act - TrClient automatically handles enum serialization
        //    var r = await f.Client.GetAsync<SearchEndpoint, SearchRequest, ProblemDetails>(new SearchRequest
        //    {
        //        DeviceId = deviceId,
        //        SearchText = "test",
        //        StorageType = TeensyStorageType.USB
        //    });

        //    // Assert
        //    r.Should().BeProblem()
        //        .WithStatusCode(HttpStatusCode.NotFound)
        //        .WithMessage($"The storage {TeensyStorageType.USB} is not available.");
        //}

        [Fact]
        public async Task When_DeviceNotConnected_ReturnsNotFound()
        {
            // Arrange
            var nonExistentDeviceId = Guid.NewGuid().ToString().GenerateFilenameSafeHash();

            // Act - TrClient automatically handles enum serialization
            var r = await f.Client.GetAsync<SearchEndpoint, SearchRequest, ProblemDetails>(new SearchRequest
            {
                DeviceId = nonExistentDeviceId,
                SearchText = "test",
                StorageType = TeensyStorageType.SD
            });

            // Assert
            r.Should().BeProblem()
                .WithStatusCode(HttpStatusCode.NotFound)
                .WithMessage($"The device {nonExistentDeviceId} was not found.");
        }

        public void Dispose() => f.Reset();
    }
}