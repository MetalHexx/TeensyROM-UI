using FluentAssertions;
using TeensyRom.Api.Endpoints.Files.FavoriteFile;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;
using static System.Net.Mime.MediaTypeNames;

namespace TeensyRom.Api.Tests.Integration
{
    [Collection("Endpoint")]
    public class RemoveFavoriteTests(EndpointFixture f) : IDisposable
    {
        private const string _nonExistentPath = "/something/that/doesnt/exist.sid";

        [Theory]
        [InlineData("/music/MUSICIANS/E/Eclipse/True.sid", "/music/MUSICIANS/E/Eclipse")]
        [InlineData("/images/Dio2.kla", "/images")]
        public async Task When_RemovingExistingFavorite_ReturnsSuccessWithMessage(string fileToFavoritePath, string preindexPath)
        {
            // Arrange              
            var deviceId = await f.GetConnectedDevice();

            // First save it as favorite
            var saveFavRequest = new SaveFavoriteRequest
            {
                DeviceId = deviceId,
                FilePath = fileToFavoritePath,
                StorageType = TeensyStorageType.SD
            };
            var (http, response) = await f.Client.PostAsync<FavoriteFileEndpoint, SaveFavoriteRequest, SaveFavoriteResponse>(saveFavRequest);

            // Act - remove the favorite
            var removeFavRequest = new RemoveFavoriteRequest
            {
                DeviceId = deviceId,
                FilePath = response.FavoriteFile.Path,
                StorageType = TeensyStorageType.SD
            };
            var r = await f.Client.DeleteAsync<RemoveFavoriteEndpoint, RemoveFavoriteRequest, RemoveFavoriteResponse>(removeFavRequest);

            // Assert  
            r.Should().BeSuccessful<RemoveFavoriteResponse>()
                .WithStatusCode(HttpStatusCode.OK)
                .WithContentNotNull();

            r.Content.Should().NotBeNull();
            r.Content.Message.Should().NotBeNullOrEmpty().And.Contain("Success");
        }

        [Fact]
        public async Task When_RemovingNonExistentFavorite_ReturnsSuccess()
        {
            // Arrange              
            var deviceId = await f.GetConnectedDevice();

            // Act
            var request = new RemoveFavoriteRequest
            {
                DeviceId = deviceId,
                FilePath = "/music/MUSICIANS/E/Eclipse/True.sid",
                StorageType = TeensyStorageType.SD
            };
            var r = await f.Client.DeleteAsync<RemoveFavoriteEndpoint, RemoveFavoriteRequest, RemoveFavoriteResponse>(request);

            // Assert
            r.Should().BeSuccessful<RemoveFavoriteResponse>()
                .WithStatusCode(HttpStatusCode.OK);                
        }

        [Fact]
        public async void When_RemoveFavoriteCalled_WithInvalidDeviceId_ReturnsNotFound()
        {
            // Act
            var request = new RemoveFavoriteRequest
            {
                DeviceId = "invalid-device-id",
                FilePath = _nonExistentPath,
                StorageType = TeensyStorageType.SD
            };
            var r = await f.Client.DeleteAsync<RemoveFavoriteEndpoint, RemoveFavoriteRequest, ValidationProblemDetails>(request);

            // Assert  
            r.Should().BeValidationProblem()
                .WithStatusCode(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async void When_RemoveFavoriteCalled_WithDeviceThatDoesntExist_ReturnsNotFound()
        {
            // Act
            var request = new RemoveFavoriteRequest
            {
                DeviceId = Guid.NewGuid().ToString().GenerateFilenameSafeHash(),
                FilePath = _nonExistentPath,
                StorageType = TeensyStorageType.SD
            };
            var r = await f.Client.DeleteAsync<RemoveFavoriteEndpoint, RemoveFavoriteRequest, ProblemDetails>(request);

            // Assert  
            r.Should().BeProblem()
                .WithStatusCode(HttpStatusCode.NotFound);
        }

        [Fact]
        public async void When_RemoveFavoriteCalled_WithNonExistentFile_ReturnsNotFound()
        {
            // Arrange              
            var deviceId = await f.GetConnectedDevice();

            // Act
            var request = new RemoveFavoriteRequest
            {
                DeviceId = deviceId,
                FilePath = _nonExistentPath,
                StorageType = TeensyStorageType.SD
            };
            var r = await f.Client.DeleteAsync<RemoveFavoriteEndpoint, RemoveFavoriteRequest, ProblemDetails>(request);

            // Assert  
            r.Should().BeProblem()
                .WithStatusCode(HttpStatusCode.NotFound);
        }

        [Fact]
        public async void When_RemoveFavoriteCalled_WithInvalidStorageType_ReturnsBadRequest()
        {
            // Act
            var request = new RemoveFavoriteRequest
            {
                DeviceId = "invalid-device-id",
                FilePath = _nonExistentPath,
                StorageType = (TeensyStorageType)999
            };
            var r = await f.Client.DeleteAsync<RemoveFavoriteEndpoint, RemoveFavoriteRequest, ProblemDetails>(request);

            // Assert  
            r.Should().BeProblem()
                .WithStatusCode(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async void When_RemoveFavoriteCalled_WithEmptyFilePath_ReturnsBadRequest()
        {
            // Arrange              
            var deviceId = await f.GetConnectedDevice();

            // Act
            var request = new RemoveFavoriteRequest
            {
                DeviceId = deviceId,
                FilePath = string.Empty,
                StorageType = TeensyStorageType.SD
            };
            var r = await f.Client.DeleteAsync<RemoveFavoriteEndpoint, RemoveFavoriteRequest, ValidationProblemDetails>(request);

            // Assert  
            r.Should().BeValidationProblem()
                .WithStatusCode(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async void When_RemoveFavoriteCalled_WithInvalidFilePath_ReturnsBadRequest()
        {
            // Arrange              
            var deviceId = await f.GetConnectedDevice();

            // Act
            var request = new RemoveFavoriteRequest
            {
                DeviceId = deviceId,
                FilePath = "invalid path with spaces",
                StorageType = TeensyStorageType.SD
            };
            var r = await f.Client.DeleteAsync<RemoveFavoriteEndpoint, RemoveFavoriteRequest, ValidationProblemDetails>(request);

            // Assert  
            r.Should().BeValidationProblem()
                .WithStatusCode(HttpStatusCode.BadRequest);
        }

        public void Dispose() => f.Reset();
    }
}
