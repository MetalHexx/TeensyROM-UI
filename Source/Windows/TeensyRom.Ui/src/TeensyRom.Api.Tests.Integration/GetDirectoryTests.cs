using System.Diagnostics;
using System.IO;
using TeensyRom.Api.Endpoints.ConnectDevice;
using TeensyRom.Api.Endpoints.Files.GetDirectory;
using TeensyRom.Api.Endpoints.FindCarts;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Api.Tests.Integration;

[Collection("Endpoint")]
public class GetDirectoryTests(EndpointFixture f) : IDisposable
{
    public static IEnumerable<object[]> ValidPaths =>
        new List<object[]>
        {
            new object[] { "/music/MUSICIANS/L/LukHash/" },
            new object[] { "/music/MUSICIANS/J/Jammic/" },
            new object[] { "/games/" }
        };


    //TODO: Come back and create a view model or solution for deserializing Interface types.
    [Fact]
    public async Task When_ValidRequest_DirectoryReturned()
    {
        // Arrange
        var deviceId = await f.GetConnectedDevice();
        var directoryPath = "/";

        // Act
        var r = await f.Client.GetAsync<GetDirectoryEndpoint, GetDirectoryRequest, GetDirectoryResponse>(new GetDirectoryRequest
        {
            DeviceId = deviceId,
            Path = directoryPath,
            StorageType = TeensyStorageType.SD
        });

        //Assert
        r.Should().BeSuccessful<GetDirectoryResponse>()
                .WithStatusCode(HttpStatusCode.OK)
                .WithContentNotNull();

        r.Should().NotBeNull();
    }


    [Fact]
    public async Task When_DeviceIdInvalid_ReturnsBadRequest()
    {
        // Act
        var r = await f.Client.GetAsync<GetDirectoryEndpoint, GetDirectoryRequest, ValidationProblemDetails>(new GetDirectoryRequest
        {
            DeviceId = "!!invalid",
            Path = "/music",
            StorageType = TeensyStorageType.SD
        });

        // Assert
        r.Should().BeValidationProblem()
            .WithKeyAndValue("DeviceId", "Device ID must be a valid filename-safe hash of 8 characters long.");
    }

    [Fact]
    public async Task When_PathInvalid_ReturnsBadRequest()
    {
        var validDeviceId = Guid.NewGuid().ToString().GenerateFilenameSafeHash();

        var r = await f.Client.GetAsync<GetDirectoryEndpoint, GetDirectoryRequest, ValidationProblemDetails>(new GetDirectoryRequest            
        {
            DeviceId = validDeviceId,
            Path = "@!bad-path!!",
            StorageType = TeensyStorageType.SD
        });

        r.Should().BeValidationProblem()
            .WithKeyAndValue("Path", "Path must be a valid Unix-style directory path.");
    }

    [Fact]
    public async Task When_MissingPath_ReturnsBadRequest()
    {
        var validDeviceId = Guid.NewGuid().ToString().GenerateFilenameSafeHash();

        var r = await f.Client.GetAsync<GetDirectoryEndpoint, GetDirectoryRequest, ValidationProblemDetails>(new GetDirectoryRequest            
        {
            DeviceId = validDeviceId,
            Path = "",
            StorageType = TeensyStorageType.SD
        });

        r.Should().BeValidationProblem()
            .WithKeyAndValue("Path", "Path is required.");
    }

    [Fact]
    public async Task When_InvalidStorageType_ReturnsBadRequest()
    {
        // Arrange
        var validDeviceId = Guid.NewGuid().ToString().GenerateFilenameSafeHash();

        // Act
        var r = await f.Client.GetAsync<GetDirectoryEndpoint, GetDirectoryRequest, ValidationProblemDetails>(new GetDirectoryRequest            
        {
            DeviceId = validDeviceId,
            Path = "/music",
            StorageType = (TeensyStorageType)999
        });

        // Assert
        r.Should().BeValidationProblem()
            .WithKeyAndValue("StorageType", "Storage type must be a valid enum value.");
    }

    [Fact]
    public async Task When_StorageNotAvailable_ReturnsNotFound()
    {
        // Arrange
        var deviceId = await f.GetConnectedDevice();
        var expectedPath = "/music";

        // Act
        var r = await f.Client.GetAsync<GetDirectoryEndpoint, GetDirectoryRequest, ProblemDetails>(new GetDirectoryRequest
        {
            DeviceId = deviceId,
            Path = expectedPath,
            StorageType = TeensyStorageType.USB
        });

        // Assert
        r.Should().BeProblem()
            .WithStatusCode(HttpStatusCode.NotFound)
            .WithMessage($"The storage {TeensyStorageType.USB} is not available.");
    }

    //TODO: Fix a bug that causes directories that are not found to be added as an empty directory in the cache.
    [Fact]
    public async Task When_DirectoryNotFound_ReturnsNotFound()
    {
        // Arrange
        var deviceId = await f.GetConnectedDevice();
        var expectedPath = "/fake/path";

        // Act
        var r = await f.Client.GetAsync<GetDirectoryEndpoint, GetDirectoryRequest, ProblemDetails>(new GetDirectoryRequest
        {
            DeviceId = deviceId,
            Path = expectedPath,
            StorageType = TeensyStorageType.SD
        });

        // Assert
        r.Should().BeProblem()
            .WithStatusCode(HttpStatusCode.NotFound)
            .WithMessage($"The directory {expectedPath} was not found.");
    }

    [Fact]
    public async Task When_Directory_IsAFilePath_BadRequestReturned()
    {
        // Arrange
        var deviceId = await f.GetConnectedDevice();
        var expectedPath = "/music/MUSICIANS/L/LukHash/Alpha.sid";

        // Act        

        var r = await f.Client.GetAsync<GetDirectoryEndpoint, GetDirectoryRequest, ProblemDetails>(new GetDirectoryRequest
        {
            DeviceId = deviceId,
            StorageType = TeensyStorageType.SD,
            Path = expectedPath
        });

        // Assert
        r.Should().BeProblem()
            .WithStatusCode(HttpStatusCode.NotFound)
            .WithMessage($"The directory {expectedPath} was not found.");            
    }

    public void Dispose() => f.Reset();
}
