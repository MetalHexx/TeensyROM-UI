using System.Net;
using System.Text.Json;
using FluentAssertions;
using TeensyRom.Api.Endpoints.FindCarts;
using TeensyRom.Api.Endpoints.GetDirectory;
using TeensyRom.Api.Endpoints.OpenPort;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Storage;

namespace TeensyRom.Api.Tests.Integration;

[Collection("Endpoint")]
public class GetDirectoryTests(EndpointFixture f)
{
    public static IEnumerable<object[]> ValidPaths =>
        new List<object[]>
        {
            new object[] { "/music/MUSICIANS/L/LukHash/" },
            new object[] { "/music/MUSICIANS/J/Jammic/" },
            new object[] { "/games/Large/" }
        };


    //TODO: Come back and create a view model or solution for deserializing Interface types.
    [Theory]
    [MemberData(nameof(ValidPaths))]
    public async Task When_ValidRequest_DirectoryReturned(string path)
    {
        // Arrange
        var deviceResult = await f.Client.GetAsync<FindCartsEndpoint, FindCartsResponse>();
        var deviceId = deviceResult.Content.AvailableCarts.First().DeviceId;

        var openPortRequest = new OpenPortRequest
        {
            DeviceId = deviceId
        };
        var openPortResponse = await f.Client.GetAsync<OpenPortEndpoint, OpenPortRequest, OpenPortResponse>(openPortRequest);

        // Arrange
        var directoryPath = Path.GetDirectoryName(path)!.Replace('\\', '/');
        var dirRequest = new GetDirectoryRequest
        {
            DeviceId = deviceId,
            Path = directoryPath,
            StorageType = TeensyStorageType.SD
        };

        // Act
        var r = await f.Client.GetAsync<GetDirectoryEndpoint, GetDirectoryRequest, dynamic>(dirRequest);

        // Assert
        r.Should().BeSuccessful<dynamic>()
            .WithStatusCode(HttpStatusCode.OK)
            .WithContentNotNull();

        r.Should().NotBeNull();
    }


    [Fact]
    public async Task When_DeviceIdInvalid_ReturnsBadRequest()
    {
        var r = await f.Client.GetAsync<GetDirectoryEndpoint, GetDirectoryRequest, ValidationProblemDetails>(
            new GetDirectoryRequest
            {
                DeviceId = "!!invalid",
                Path = "/music",
                StorageType = TeensyStorageType.SD
            });

        r.Should().BeValidationProblem()
            .WithKeyAndValue("DeviceId", "Device ID must be a valid filename-safe hash of 8 characters long.");
    }

    [Fact]
    public async Task When_PathInvalid_ReturnsBadRequest()
    {
        var validDeviceId = Guid.NewGuid().ToString().GenerateFilenameSafeHash();

        var r = await f.Client.GetAsync<GetDirectoryEndpoint, GetDirectoryRequest, ValidationProblemDetails>(
            new GetDirectoryRequest
            {
                DeviceId = validDeviceId,
                Path = "@!bad-path!!",
                StorageType = TeensyStorageType.SD
            });

        r.Should().BeValidationProblem()
            .WithKeyAndValue("Path", "Path must be a valid Unix-style file path.");
    }

    [Fact]
    public async Task When_MissingDeviceId_ReturnsBadRequest()
    {
        var r = await f.Client.GetAsync<GetDirectoryEndpoint, GetDirectoryRequest, ValidationProblemDetails>(
            new GetDirectoryRequest
            {
                DeviceId = "",
                Path = "/music",
                StorageType = TeensyStorageType.SD
            });

        r.Should().BeValidationProblem()
            .WithKeyAndValue("DeviceId", "Device ID must be a valid filename-safe hash of 8 characters long.");
    }

    [Fact]
    public async Task When_MissingPath_ReturnsBadRequest()
    {
        var validDeviceId = Guid.NewGuid().ToString().GenerateFilenameSafeHash();

        var r = await f.Client.GetAsync<GetDirectoryEndpoint, GetDirectoryRequest, ValidationProblemDetails>(
            new GetDirectoryRequest
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
        var validDeviceId = Guid.NewGuid().ToString().GenerateFilenameSafeHash();

        var r = await f.Client.GetAsync<GetDirectoryEndpoint, GetDirectoryRequest, ValidationProblemDetails>(
            new GetDirectoryRequest
            {
                DeviceId = validDeviceId,
                Path = "/music",
                StorageType = (TeensyStorageType)999
            });

        r.Should().BeValidationProblem()
            .WithKeyAndValue("StorageType", "Storage type must be a valid enum value.");
    }

    [Fact]
    public async Task When_StorageNotAvailable_ReturnsNotFound()
    {
        // Arrange
        var cart = await f.Client.GetAsync<FindCartsEndpoint, FindCartsResponse>();
        var device = cart.Content.AvailableCarts.First();

        await f.Client.GetAsync<OpenPortEndpoint, OpenPortRequest, OpenPortResponse>(new OpenPortRequest
        {
            DeviceId = device.DeviceId!
        });

        var request = new GetDirectoryRequest
        {
            DeviceId = device.DeviceId,
            Path = "/music",
            StorageType = TeensyStorageType.USB 
        };

        // Act
        var r = await f.Client.GetAsync<GetDirectoryEndpoint, GetDirectoryRequest, ProblemDetails>(request);

        // Assert
        r.Should().BeProblem().WithStatusCode(HttpStatusCode.NotFound);
        r.Content.Title.Should().Contain("not available");
    }

    //TODO: Fix a bug that causes directories that are not found to be added as an empty directory in the cache.
    [Fact]
    public async Task When_DirectoryNotFound_ReturnsNotFound()
    {
        // Arrange
        var cart = await f.Client.GetAsync<FindCartsEndpoint, FindCartsResponse>();
        var device = cart.Content.AvailableCarts.First();

        await f.Client.GetAsync<OpenPortEndpoint, OpenPortRequest, OpenPortResponse>(new OpenPortRequest
        {
            DeviceId = device.DeviceId!
        });

        var request = new GetDirectoryRequest
        {
            DeviceId = device.DeviceId,
            Path = "/not/a/real/path",
            StorageType = TeensyStorageType.SD
        };

        // Act
        var r = await f.Client.GetAsync<GetDirectoryEndpoint, GetDirectoryRequest, ProblemDetails>(request);

        // Assert
        r.Should().BeProblem().WithStatusCode(HttpStatusCode.NotFound);
        r.Content.Title.Should().Contain("was not found");
    }
}
