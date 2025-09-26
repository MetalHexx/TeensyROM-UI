using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using TeensyRom.Api.Endpoints.Player.LaunchRandom;
using TeensyRom.Api.Endpoints.ResetDevice;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Settings;

namespace TeensyRom.Api.Tests.Integration
{

    [Collection("Endpoint")]
    public class LaunchRandomTests(EndpointFixture f) : IDisposable
    {
        public const string Games_Path = "/games/";
        public const string Very_Large_Games_Path = "/games/Very Large/";
        public const string Music_Path = "/music/MUSICIANS/L/LukHash/";
        public const string Images_Path = "/images/";

        [Fact]
        public async Task When_DeviceIsInvalidFormat_ReturnsBadRequest()
        {
            // Arrange
            var request = new LaunchRandomRequest
            {
                DeviceId = "invalid@@",
                StorageType = TeensyStorageType.SD
            };

            // Act - TrClient automatically handles enum serialization
            var r = await f.Client.PostAsync<LaunchRandomEndpoint, LaunchRandomRequest, ValidationProblemDetails>(request);

            // Assert
            r.Should().BeValidationProblem()
                .WithKeyAndValue("DeviceId", "Device ID must be a valid filename-safe hash of 8 characters long.");
        }

        [Fact]
        public async Task When_PathIsInvalid_ReturnsBadRequest()
        {
            // Arrange
            var request = new LaunchRandomRequest
            {
                DeviceId = "PSM2ZAKI",
                StorageType = TeensyStorageType.SD,
                StartingDirectory = ")(#$*#h"
            };

            // Act - TrClient automatically handles enum serialization
            var r = await f.Client.PostAsync<LaunchRandomEndpoint, LaunchRandomRequest, ValidationProblemDetails>(request);

            // Assert
            r.Should().BeValidationProblem()
                .WithKeyAndValue("StartingDirectory", "Path must be a valid Unix-style file path.");
        }

        [Fact]
        public async Task When_EnumValueIsInvalid_ReturnsBadRequest()
        {
            // Arrange
            var request = new LaunchRandomRequest
            {
                DeviceId = "PSM2ZAKI",
                StorageType = (TeensyStorageType)999,
                FilterType = (TeensyFilterType)999,
                Scope = (StorageScope)999
            };

            // Act - TrClient automatically handles enum serialization
            var r = await f.Client.PostAsync<LaunchRandomEndpoint, LaunchRandomRequest, ValidationProblemDetails>(request);

            // Assert
            r.Should().BeValidationProblem()
                .WithKey("StorageType")
                .WithKey("FilterType")
                .WithKey("Scope");
        }

        [Fact]
        public async Task When_DeviceNotConnected_ReturnsNotFound()
        {
            // Arrange
            var deviceId = "PSM2ZAKI";
            var request = new LaunchRandomRequest
            {
                DeviceId = deviceId,
                StorageType = TeensyStorageType.SD
            };

            // Act - TrClient automatically handles enum serialization
            var r = await f.Client.PostAsync<LaunchRandomEndpoint, LaunchRandomRequest, ProblemDetails>(request);

            // Assert
            r.Should().BeProblem()
                .WithStatusCode(HttpStatusCode.NotFound)
                .WithMessage($"The device {deviceId} was not found.");
        }

        [Fact]
        public async Task When_StorageTypeNotAvailable_ReturnsNotFound()
        {
            // Arrange
            var deviceId = await f.GetConnectedDevice();
            var request = new LaunchRandomRequest
            {
                DeviceId = deviceId,
                StorageType = TeensyStorageType.USB
            };

            // Act - TrClient automatically handles enum serialization
            var r = await f.Client.PostAsync<LaunchRandomEndpoint, LaunchRandomRequest, ProblemDetails>(request);

            // Assert
            r.Should().BeProblem()
                .WithStatusCode(HttpStatusCode.NotFound)
                .WithMessage($"The device {deviceId} does not have USB storage.");
        }

        [Fact]
        public async Task When_NoFilesFound_ReturnsNotFound()
        {
            // Arrange
            var deviceId = await f.GetConnectedDevice();
            var request = new LaunchRandomRequest
            {
                DeviceId = deviceId,
                StorageType = TeensyStorageType.SD,
                StartingDirectory = "/empty"
            };

            // Act - TrClient automatically handles enum serialization
            var r = await f.Client.PostAsync<LaunchRandomEndpoint, LaunchRandomRequest, ProblemDetails>(request);

            // Assert
            r.Should().BeProblem()
                .WithStatusCode(HttpStatusCode.NotFound)
                .WithMessage("No files were found.");
        }

        [Fact]
        public async Task When_ValidGame_Request_With_DeepScope_LaunchesRandomGame()
        {
            // Arrange
            var deviceId = await f.GetConnectedDevice();
            f.DeleteCache(deviceId!, TeensyStorageType.SD);
            await f.Preindex(deviceId, TeensyStorageType.SD, Games_Path);            

            var request = new LaunchRandomRequest
            {
                DeviceId = deviceId,
                StorageType = TeensyStorageType.SD,
                StartingDirectory = Games_Path,
                FilterType = TeensyFilterType.All,
                Scope = StorageScope.DirDeep
            };

            // Act - TrClient automatically handles enum serialization
            var r = await f.Client.PostAsync<LaunchRandomEndpoint, LaunchRandomRequest, LaunchRandomResponse>(request);

            await Task.Delay(3000);

            // Assert
            r.Should().BeSuccessful<LaunchRandomResponse>()
                .WithStatusCode(HttpStatusCode.OK)
                .WithContentNotNull();

            r.Content.LaunchedFile.Should().NotBeNull();
            r.Content.LaunchedFile.Path.Should().StartWith(Games_Path);
            r.Content.Message.Should().Contain("Success");
        }

        [Fact]
        public async Task When_ValidGame_Request_With_ShallowScope_LaunchesRandomGame()
        {
            // Arrange
            var deviceId = await f.GetConnectedDevice();
            f.DeleteCache(deviceId!, TeensyStorageType.SD);
            await f.Preindex(deviceId, TeensyStorageType.SD, Games_Path);
            var request = new LaunchRandomRequest
            {
                DeviceId = deviceId,
                StorageType = TeensyStorageType.SD,
                StartingDirectory = Games_Path,
                FilterType = TeensyFilterType.All,
                Scope = StorageScope.DirShallow
            };
            // Act - TrClient automatically handles enum serialization
            var r = await f.Client.PostAsync<LaunchRandomEndpoint, LaunchRandomRequest, LaunchRandomResponse>(request);
            await Task.Delay(3000);

            // Assert
            r.Should().BeSuccessful<LaunchRandomResponse>()
                .WithStatusCode(HttpStatusCode.OK)
                .WithContentNotNull();

            var actualPath = r.Content.LaunchedFile.Path.GetUnixParentPath().EnsureUnixPathEnding();

            r.Content.LaunchedFile.Should().NotBeNull();
            actualPath.Should().Be(Games_Path);
            r.Content.Message.Should().Contain("Success");
        }

        [Fact]
        public async Task When_ValidGame_Request_With_ShallowScope_And_VeryLargeGame_LaunchesRandomGame()
        {
            // Arrange
            var deviceId = await f.GetConnectedDevice();
            f.DeleteCache(deviceId!, TeensyStorageType.SD);
            await f.Preindex(deviceId, TeensyStorageType.SD, Very_Large_Games_Path);

            var request = new LaunchRandomRequest
            {
                DeviceId = deviceId,
                StorageType = TeensyStorageType.SD,
                StartingDirectory = Very_Large_Games_Path,
                FilterType = TeensyFilterType.All,
                Scope = StorageScope.DirShallow
            };
            // Act - TrClient automatically handles enum serialization
            var r = await f.Client.PostAsync<LaunchRandomEndpoint, LaunchRandomRequest, LaunchRandomResponse>(request);
            await Task.Delay(3000);

            // Assert
            r.Should().BeSuccessful<LaunchRandomResponse>()
                .WithStatusCode(HttpStatusCode.OK)
                .WithContentNotNull();

            var actualPath = r.Content.LaunchedFile.Path.GetUnixParentPath().EnsureUnixPathEnding();

            r.Content.LaunchedFile.Should().NotBeNull();
            actualPath.Should().Be(Very_Large_Games_Path);
            r.Content.Message.Should().Contain("Success");

            // Cleanup
            await f.ResetDevice(deviceId);
        }

        [Fact]
        public async Task When_ValidGame_Request_With_StorageScope_LaunchesRandomGame()
        {
            // Arrange
            var deviceId = await f.GetConnectedDevice();
            f.DeleteCache(deviceId!, TeensyStorageType.SD);
            await f.Preindex(deviceId, TeensyStorageType.SD, Games_Path);

            var request = new LaunchRandomRequest
            {
                DeviceId = deviceId,
                StorageType = TeensyStorageType.SD,
                StartingDirectory = Games_Path,
                FilterType = TeensyFilterType.All,
                Scope = StorageScope.Storage
            };

            // Act - TrClient automatically handles enum serialization
            var r = await f.Client.PostAsync<LaunchRandomEndpoint, LaunchRandomRequest, LaunchRandomResponse>(request);
            await Task.Delay(3000);

            // Assert
            r.Should().BeSuccessful<LaunchRandomResponse>()
                .WithStatusCode(HttpStatusCode.OK)
                .WithContentNotNull();

            r.Content.LaunchedFile.Should().NotBeNull();
            r.Content.LaunchedFile.Path.Should().Contain(Games_Path);
            r.Content.Message.Should().Contain("Success");
        }

        [Fact]
        public async Task When_ValidMusic_Request_With_DeepScope_LaunchesRandomSong()
        {
            // Arrange
            var deviceId = await f.GetConnectedDevice();
            f.DeleteCache(deviceId!, TeensyStorageType.SD);
            await f.Preindex(deviceId, TeensyStorageType.SD, Music_Path);

            var request = new LaunchRandomRequest
            {
                DeviceId = deviceId,
                StorageType = TeensyStorageType.SD,
                StartingDirectory = Music_Path,
                FilterType = TeensyFilterType.All,
                Scope = StorageScope.DirDeep
            };

            // Act - TrClient automatically handles enum serialization
            var r = await f.Client.PostAsync<LaunchRandomEndpoint, LaunchRandomRequest, LaunchRandomResponse>(request);
            await Task.Delay(3000);

            // Assert
            r.Should().BeSuccessful<LaunchRandomResponse>()
                .WithStatusCode(HttpStatusCode.OK)
                .WithContentNotNull();
            r.Content.LaunchedFile.Should().NotBeNull();
            r.Content.LaunchedFile.Path.Should().StartWith(Music_Path);
            r.Content.Message.Should().Contain("Success");
        }

        [Fact]
        public async Task When_ValidMusic_Request_With_ShallowScope_LaunchesRandomSong()
        {
            // Arrange
            var deviceId = await f.GetConnectedDevice();
            f.DeleteCache(deviceId!, TeensyStorageType.SD);
            await f.Preindex(deviceId, TeensyStorageType.SD, Music_Path);

            var request = new LaunchRandomRequest
            {
                DeviceId = deviceId,
                StorageType = TeensyStorageType.SD,
                StartingDirectory = Music_Path,
                FilterType = TeensyFilterType.All,
                Scope = StorageScope.DirShallow
            };

            // Act - TrClient automatically handles enum serialization
            var r = await f.Client.PostAsync<LaunchRandomEndpoint, LaunchRandomRequest, LaunchRandomResponse>(request);
            await Task.Delay(3000);

            // Assert
            r.Should().BeSuccessful<LaunchRandomResponse>()
                .WithStatusCode(HttpStatusCode.OK)
                .WithContentNotNull();

            r.Content.LaunchedFile.Should().NotBeNull();
            r.Content.LaunchedFile.Path.Should().Contain(Music_Path);
            r.Content.Message.Should().Contain("Success");
        }

        [Fact]
        public async Task When_ValidMusic_Request_With_StorageScope_LaunchesRandomSong()
        {
            // Arrange
            var deviceId = await f.GetConnectedDevice();
            f.DeleteCache(deviceId!, TeensyStorageType.SD);
            await f.Preindex(deviceId, TeensyStorageType.SD, Music_Path);

            var request = new LaunchRandomRequest
            {
                DeviceId = deviceId,
                StorageType = TeensyStorageType.SD,
                StartingDirectory = Music_Path,
                FilterType = TeensyFilterType.All,
                Scope = StorageScope.Storage
            };

            // Act - TrClient automatically handles enum serialization
            var r = await f.Client.PostAsync<LaunchRandomEndpoint, LaunchRandomRequest, LaunchRandomResponse>(request);
            await Task.Delay(3000);

            // Assert
            r.Should().BeSuccessful<LaunchRandomResponse>()
                .WithStatusCode(HttpStatusCode.OK)
                .WithContentNotNull();

            var actualPath = r.Content.LaunchedFile.Path.GetUnixParentPath().EnsureUnixPathEnding();

            r.Content.LaunchedFile.Should().NotBeNull();            
            actualPath.Should().Be(Music_Path);
            r.Content.Message.Should().Contain("Success");
        }

        [Fact]
        public async Task When_ValidImage_Request_With_DeepScope_LaunchesRandomImage()
        {
            // Arrange
            var deviceId = await f.GetConnectedDevice();
            f.DeleteCache(deviceId!, TeensyStorageType.SD);
            await f.Preindex(deviceId, TeensyStorageType.SD, Images_Path);

            var request = new LaunchRandomRequest
            {
                DeviceId = deviceId,
                StorageType = TeensyStorageType.SD,
                StartingDirectory = Images_Path,
                FilterType = TeensyFilterType.All,
                Scope = StorageScope.DirDeep
            };

            // Act - TrClient automatically handles enum serialization
            var r = await f.Client.PostAsync<LaunchRandomEndpoint, LaunchRandomRequest, LaunchRandomResponse>(request);
            await Task.Delay(3000);

            // Assert
            r.Should().BeSuccessful<LaunchRandomResponse>()
                .WithStatusCode(HttpStatusCode.OK)
                .WithContentNotNull();
            r.Content.LaunchedFile.Should().NotBeNull();
            r.Content.LaunchedFile.Path.Should().StartWith(Images_Path);
            r.Content.Message.Should().Contain("Success");
        }

        [Fact]
        public async Task When_ValidImage_Request_With_ShallowScope_LaunchesRandomImage()
        {
            // Arrange
            var deviceId = await f.GetConnectedDevice();
            f.DeleteCache(deviceId!, TeensyStorageType.SD);
            await f.Preindex(deviceId, TeensyStorageType.SD, Images_Path);

            var request = new LaunchRandomRequest
            {
                DeviceId = deviceId,
                StorageType = TeensyStorageType.SD,
                StartingDirectory = Images_Path,
                FilterType = TeensyFilterType.All,
                Scope = StorageScope.DirShallow
            };

            // Act - TrClient automatically handles enum serialization
            var r = await f.Client.PostAsync<LaunchRandomEndpoint, LaunchRandomRequest, LaunchRandomResponse>(request);
            await Task.Delay(3000);

            // Assert
            r.Should().BeSuccessful<LaunchRandomResponse>()
                .WithStatusCode(HttpStatusCode.OK)
                .WithContentNotNull();

            var actualPath = r.Content.LaunchedFile.Path.GetUnixParentPath().EnsureUnixPathEnding();

            r.Content.LaunchedFile.Should().NotBeNull();
            actualPath.Should().Be(Images_Path);
            r.Content.Message.Should().Contain("Success");
        }

        [Fact]
        public async Task When_ValidImage_Request_With_StorageScope_LaunchesRandomImage()
        {
            // Arrange
            var deviceId = await f.GetConnectedDevice();
            f.DeleteCache(deviceId!, TeensyStorageType.SD);
            await f.Preindex(deviceId, TeensyStorageType.SD, Images_Path);

            var request = new LaunchRandomRequest
            {
                DeviceId = deviceId,
                StorageType = TeensyStorageType.SD,
                StartingDirectory = Images_Path,
                FilterType = TeensyFilterType.All,
                Scope = StorageScope.Storage
            };

            // Act - TrClient automatically handles enum serialization
            var r = await f.Client.PostAsync<LaunchRandomEndpoint, LaunchRandomRequest, LaunchRandomResponse>(request);
            await Task.Delay(3000);

            // Assert
            r.Should().BeSuccessful<LaunchRandomResponse>()
                .WithStatusCode(HttpStatusCode.OK)
                .WithContentNotNull();

            var actualPath = r.Content.LaunchedFile.Path.GetUnixParentPath().EnsureUnixPathEnding();

            r.Content.LaunchedFile.Should().NotBeNull();
            actualPath.Should().Be(Images_Path);
            r.Content.Message.Should().Contain("Success");
        }

        [Fact]
        public async Task When_ValidGame_Request_With_GameFilter_LaunchesRandomGame()
        {
            // Arrange
            var deviceId = await f.GetConnectedDevice();
            f.DeleteCache(deviceId!, TeensyStorageType.SD);
            await f.Preindex(deviceId, TeensyStorageType.SD, Games_Path);

            var request = new LaunchRandomRequest
            {
                DeviceId = deviceId,
                StorageType = TeensyStorageType.SD,
                FilterType = TeensyFilterType.Games
            };

            // Act - TrClient automatically handles enum serialization
            var r = await f.Client.PostAsync<LaunchRandomEndpoint, LaunchRandomRequest, LaunchRandomResponse>(request);
            await Task.Delay(3000);

            // Assert
            r.Should().BeSuccessful<LaunchRandomResponse>()
                .WithStatusCode(HttpStatusCode.OK)
                .WithContentNotNull();

            r.Content.LaunchedFile.Should().NotBeNull();
            r.Content.LaunchedFile.Type.Should().Be(Models.FileItemType.Game);
            r.Content.Message.Should().Contain("Success");
        }

        [Fact]
        public async Task When_ValidImage_Request_With_ImageFilter_LaunchesRandomImage()
        {
            // Arrange
            var deviceId = await f.GetConnectedDevice();
            f.DeleteCache(deviceId!, TeensyStorageType.SD);
            await f.Preindex(deviceId, TeensyStorageType.SD, Images_Path);

            var request = new LaunchRandomRequest
            {
                DeviceId = deviceId,
                StorageType = TeensyStorageType.SD,
                FilterType = TeensyFilterType.Images
            };

            // Act - TrClient automatically handles enum serialization
            var r = await f.Client.PostAsync<LaunchRandomEndpoint, LaunchRandomRequest, LaunchRandomResponse>(request);
            await Task.Delay(3000);

            // Assert
            r.Should().BeSuccessful<LaunchRandomResponse>()
                .WithStatusCode(HttpStatusCode.OK)
                .WithContentNotNull();

            r.Content.LaunchedFile.Should().NotBeNull();
            r.Content.LaunchedFile.Type.Should().Be(Models.FileItemType.Image);
            r.Content.Message.Should().Contain("Success");
        }

        [Fact]
        public async Task When_ValidMusic_Request_With_MusicFilter_LaunchesRandomSong()
        {
            // Arrange
            var deviceId = await f.GetConnectedDevice();
            f.DeleteCache(deviceId!, TeensyStorageType.SD);
            await f.Preindex(deviceId, TeensyStorageType.SD, Music_Path);

            var request = new LaunchRandomRequest
            {
                DeviceId = deviceId,
                StorageType = TeensyStorageType.SD,
                FilterType = TeensyFilterType.Music
            };

            // Act - TrClient automatically handles enum serialization
            var r = await f.Client.PostAsync<LaunchRandomEndpoint, LaunchRandomRequest, LaunchRandomResponse>(request);
            await Task.Delay(3000);

            // Assert
            r.Should().BeSuccessful<LaunchRandomResponse>()
                .WithStatusCode(HttpStatusCode.OK)
                .WithContentNotNull();

            r.Content.LaunchedFile.Should().NotBeNull();
            r.Content.LaunchedFile.Type.Should().Be(Models.FileItemType.Song);
            r.Content.Message.Should().Contain("Success");
        }

        public void Dispose() => f.Reset();
    }
}
