using FluentAssertions;
using TeensyRom.Api.Endpoints.Files.FavoriteFile;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;
using static System.Net.Mime.MediaTypeNames;

namespace TeensyRom.Api.Tests.Integration
{
    [Collection("Endpoint")]
    public class SaveFavoriteTests(EndpointFixture f) : IDisposable
    {
        private const string _nonExistentPath = "/something/that/doesnt/exist.sid";

        [Theory]
        [InlineData("/music/MUSICIANS/E/Eclipse/True.sid", "/favorites/music/")]
        //[InlineData("/games/Large/706k The Secret of Monkey Island (D42) [EasyFlash].crt", "/favorites/games/")]
        [InlineData("/images/Dio2.kla", "/favorites/images/")]
        public async Task When_SavingValidFile_ReturnsSuccessWithAllResponsePropertiesPopulated(
            string filePath, string expectedFavoritePath)
        {
            // Arrange              
            var deviceId = await f.GetConnectedDevice();

            // Act
            var request = new SaveFavoriteRequest
            {
                DeviceId = deviceId,
                FilePath = filePath,
                StorageType = TeensyStorageType.SD
            };
            var r = await f.Client.PostAsync<FavoriteFileEndpoint, SaveFavoriteRequest, SaveFavoriteResponse>(request);

            // Assert  
            r.Should().BeSuccessful<SaveFavoriteResponse>()
                .WithStatusCode(HttpStatusCode.OK)
                .WithContentNotNull();

            r.Content.Should().NotBeNull();
            r.Content.Message.Should().NotBeNullOrEmpty().And.Contain("Favorite tagged and saved successfully to").And.Contain(expectedFavoritePath);
            r.Content.FavoritePath.Should().Be(expectedFavoritePath);
            
            r.Content.FavoriteFile.Should().NotBeNull();
            r.Content.FavoriteFile.Name.Should().Be(filePath.Split('/').Last());
            r.Content.FavoriteFile.Path.Should().StartWith(expectedFavoritePath);
            r.Content.FavoriteFile.IsFavorite.Should().BeTrue();
            r.Content.FavoriteFile.Size.Should().BeGreaterThan(0);
        }

        
        [Fact]
        public async void When_SaveFavoriteCalled_WithInvalidDeviceId_ReturnsNotFound()
        {
            // Act
            var request = new SaveFavoriteRequest
            {
                DeviceId = "invalid-device-id",
                FilePath = _nonExistentPath,
                StorageType = TeensyStorageType.SD
            };
            var r = await f.Client.PostAsync<FavoriteFileEndpoint, SaveFavoriteRequest, ValidationProblemDetails>(request);

            // Assert  
            r.Should().BeValidationProblem()
                .WithStatusCode(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async void When_SaveFavoriteCalled_WithDeviceThatDoesntExist_ReturnsNotFound()
        {
            // Act
            var request = new SaveFavoriteRequest
            {
                DeviceId = Guid.NewGuid().ToString().GenerateFilenameSafeHash(),
                FilePath = _nonExistentPath,
                StorageType = TeensyStorageType.SD
            };
            var r = await f.Client.PostAsync<FavoriteFileEndpoint, SaveFavoriteRequest, ProblemDetails>(request);

            // Assert  
            r.Should().BeProblem()
                .WithStatusCode(HttpStatusCode.NotFound);
        }

        [Fact]
        public async void When_SaveFavoriteCalled_WithNonExistentFile_ReturnsNotFound()
        {
            // Arrange              
            var deviceId = await f.GetConnectedDevice();

            // Act
            var request = new SaveFavoriteRequest
            {
                DeviceId = deviceId,
                FilePath = _nonExistentPath,
                StorageType = TeensyStorageType.SD
            };
            var r = await f.Client.PostAsync<FavoriteFileEndpoint, SaveFavoriteRequest, ProblemDetails>(request);

            // Assert  
            r.Should().BeProblem()
                .WithStatusCode(HttpStatusCode.NotFound);
        }

        [Fact]
        public async void When_SaveFavoriteCalled_WithInvalidStorageType_ReturnsBadRequest()
        {
            // Act
            var request = new SaveFavoriteRequest
            {
                DeviceId = "invalid-device-id",
                FilePath = _nonExistentPath,
                StorageType = (TeensyStorageType)999
            };
            var r = await f.Client.PostAsync<FavoriteFileEndpoint, SaveFavoriteRequest, ProblemDetails>(request);

            // Assert  
            r.Should().BeProblem()
                .WithStatusCode(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async void When_SaveFavoriteCalled_WithEmptyFilePath_ReturnsBadRequest()
        {
            // Arrange              
            var deviceId = await f.GetConnectedDevice();

            // Act
            var request = new SaveFavoriteRequest
            {
                DeviceId = deviceId,
                FilePath = string.Empty,
                StorageType = TeensyStorageType.SD
            };
            var r = await f.Client.PostAsync<FavoriteFileEndpoint, SaveFavoriteRequest, ValidationProblemDetails>(request);

            // Assert  
            r.Should().BeValidationProblem()
                .WithStatusCode(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async void When_SaveFavoriteCalled_WithInvalidFilePath_ReturnsBadRequest()
        {
            // Arrange              
            var deviceId = await f.GetConnectedDevice();

            // Act
            var request = new SaveFavoriteRequest
            {
                DeviceId = deviceId,
                FilePath = "invalid path with spaces",
                StorageType = TeensyStorageType.SD
            };
            var r = await f.Client.PostAsync<FavoriteFileEndpoint, SaveFavoriteRequest, ValidationProblemDetails>(request);

            // Assert  
            r.Should().BeValidationProblem()
                .WithStatusCode(HttpStatusCode.BadRequest);
        }

        public void Dispose() 
        {
            f.Reset();
        }
    }
}
