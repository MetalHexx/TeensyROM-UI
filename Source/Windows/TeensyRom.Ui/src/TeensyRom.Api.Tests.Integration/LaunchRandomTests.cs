using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using TeensyRom.Api.Endpoints.Files.LaunchRandom;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Settings;

namespace TeensyRom.Api.Tests.Integration
{

    [Collection("Endpoint")]
    public class LaunchRandomTests(EndpointFixture f) : IDisposable
    {
        [Fact]
        public async Task When_DeviceIsInvalidFormat_ReturnsBadRequest()
        {
            // Arrange
            var request = new LaunchRandomRequest
            {
                DeviceId = "invalid@@",
                StorageType = TeensyStorageType.SD
            };

            // Act
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

            // Act
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

            // Act
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

            // Act
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
            var deviceId = await f.ConnectToFirstDevice();
            var request = new LaunchRandomRequest
            {
                DeviceId = deviceId,
                StorageType = TeensyStorageType.USB
            };

            // Act
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
            var deviceId = await f.ConnectToFirstDevice();
            var request = new LaunchRandomRequest
            {
                DeviceId = deviceId,
                StorageType = TeensyStorageType.SD,
                StartingDirectory = "/empty"
            };

            // Act
            var r = await f.Client.PostAsync<LaunchRandomEndpoint, LaunchRandomRequest, ProblemDetails>(request);

            // Assert
            r.Should().BeProblem()
                .WithStatusCode(HttpStatusCode.NotFound)
                .WithMessage("No files were found.");
        }

        [Fact]
        public async Task When_ValidRequest_LaunchesRandomFile()
        {
            // Arrange
            var deviceId = await f.ConnectToFirstDevice();
            await f.Preindex(deviceId, TeensyStorageType.SD, "/games");

            var request = new LaunchRandomRequest
            {
                DeviceId = deviceId,
                StorageType = TeensyStorageType.SD,
                StartingDirectory = "/games",
                FilterType = TeensyFilterType.All,
                Scope = StorageScope.DirDeep
            };

            // Act
            var r = await f.Client.PostAsync<LaunchRandomEndpoint, LaunchRandomRequest, LaunchRandomResponse>(request);

            await Task.Delay(3000);

            // Assert
            r.Should().BeSuccessful<LaunchRandomResponse>()
                .WithStatusCode(HttpStatusCode.OK)
                .WithContentNotNull();

            r.Content.LaunchedFile.Should().NotBeNull();
            r.Content.Message.Should().Contain("Success");
        }

        public void Dispose() => f.Reset();
    }
}
