using FluentAssertions;
using TeensyRom.Api.Endpoints.Player.LaunchFile;
using TeensyRom.Api.Endpoints.Player.ToggleMusic;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;
using System.Net;

namespace TeensyRom.Api.Tests.Integration
{
    [Collection("Endpoint")]
    public class ToggleMusicTests(EndpointFixture f) : IDisposable
    {
        private const string TestSidFile = "/music/MUSICIANS/L/LukHash/Alpha.sid";

        [Fact]
        public async Task When_ToggleMusicCalled_WithValidDevice_ReturnsSuccess()
        {
            // Arrange
            var deviceId = await f.GetConnectedDevice();

            // First launch a SID file so we have something to toggle
            var launchRequest = new LaunchFileRequest
            {
                DeviceId = deviceId,
                FilePath = TestSidFile,
                StorageType = TeensyStorageType.SD
            };
            await f.Client.PostAsync<LaunchFileEndpoint, LaunchFileRequest, LaunchFileResponse>(launchRequest);

            // Wait a bit for the file to start playing
            

            await f.Client.PostAsync<LaunchFileEndpoint, LaunchFileRequest, LaunchFileResponse>(launchRequest);

            await Task.Delay(2000);

            // Act - Toggle the music
            var toggleRequest = new ToggleMusicRequest
            {
                DeviceId = deviceId
            };
            var r = await f.Client.PostAsync<ToggleMusicEndpoint, ToggleMusicRequest, ToggleMusicResponse>(toggleRequest);

            // Assert
            r.Should().BeSuccessful<ToggleMusicResponse>()
                .WithStatusCode(HttpStatusCode.OK)
                .WithContentNotNull();

            r.Content.Should().NotBeNull();
            r.Content.Message.Should().Contain("successful");
        }

        [Fact]
        public async Task When_ToggleMusicCalled_WithInvalidDeviceId_ReturnsValidationError()
        {
            // Act
            var request = new ToggleMusicRequest
            {
                DeviceId = "invalid-device-id"
            };
            var r = await f.Client.PostAsync<ToggleMusicEndpoint, ToggleMusicRequest, ValidationProblemDetails>(request);

            // Assert
            r.Should().BeValidationProblem()
                .WithStatusCode(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task When_ToggleMusicCalled_WithDeviceThatDoesntExist_ReturnsNotFound()
        {
            // Act
            var request = new ToggleMusicRequest
            {
                DeviceId = Guid.NewGuid().ToString().GenerateFilenameSafeHash()
            };
            var r = await f.Client.PostAsync<ToggleMusicEndpoint, ToggleMusicRequest, ProblemDetails>(request);

            // Assert
            r.Should().BeProblem()
                .WithStatusCode(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task When_ToggleMusicCalled_MultipleTimesInSequence_ReturnsSuccessEachTime()
        {
            // Arrange
            var deviceId = await f.GetConnectedDevice();

            // First launch a SID file
            var launchRequest = new LaunchFileRequest
            {
                DeviceId = deviceId,
                FilePath = TestSidFile,
                StorageType = TeensyStorageType.SD
            };
            await f.Client.PostAsync<LaunchFileEndpoint, LaunchFileRequest, LaunchFileResponse>(launchRequest);

            // Wait a bit for the file to start playing
            await Task.Delay(2000);

            // Act - Toggle multiple times (pause, resume, pause, resume)
            var toggleRequest = new ToggleMusicRequest { DeviceId = deviceId };

            for (int i = 0; i < 4; i++)
            {
                var r = await f.Client.PostAsync<ToggleMusicEndpoint, ToggleMusicRequest, ToggleMusicResponse>(toggleRequest);
                
                // Assert each toggle operation is successful
                r.Should().BeSuccessful<ToggleMusicResponse>()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithContentNotNull();

                r.Content.Should().NotBeNull();
                r.Content.Message.Should().Contain("successful");

                // Wait a bit between toggles
                await Task.Delay(1000);
            }
        }

        public void Dispose() => f.Reset();
    }
}