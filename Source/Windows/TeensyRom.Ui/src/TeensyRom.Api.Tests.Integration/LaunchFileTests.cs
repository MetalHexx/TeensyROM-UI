using FluentAssertions;
using TeensyRom.Api.Endpoints.ClosePort;
using TeensyRom.Api.Endpoints.Files.LaunchFile;
using TeensyRom.Api.Endpoints.FindCarts;
using TeensyRom.Api.Endpoints.OpenPort;
using TeensyRom.Api.Endpoints.ResetDevice;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;
using static System.Net.Mime.MediaTypeNames;

namespace TeensyRom.Api.Tests.Integration
{
    [Collection("Endpoint")]
    public class LaunchFileTests(EndpointFixture f) : IDisposable
    {
        private const string NonExistentPath = "/something/that/doesnt/exist.sid";        

        [Fact]
        public async void When_LaunchingVariousFiles_ReturnsSuccess()
        {
            // Arrange              
            var deviceId = await f.ConnectToFirstDevice();

            var r = await f.Client.GetAsync<LaunchFileEndpoint, LaunchFileRequest, LaunchFileResponse>(new LaunchFileRequest
            {
                DeviceId = deviceId,
                FilePath = "/music/MUSICIANS/L/LukHash/Alpha.sid",
                StorageType = TeensyStorageType.SD
            });

            await Task.Delay(3000);

            r = await f.Client.GetAsync<LaunchFileEndpoint, LaunchFileRequest, LaunchFileResponse>(new LaunchFileRequest
            {
                DeviceId = deviceId,
                FilePath = "/images/Dio2.kla",
                StorageType = TeensyStorageType.SD
            });

            await Task.Delay(3000);

            r = await f.Client.GetAsync<LaunchFileEndpoint, LaunchFileRequest, LaunchFileResponse>(new LaunchFileRequest
            {
                DeviceId = deviceId,
                FilePath = "/music/MUSICIANS/J/Jammic/Wasted_Years.sid",
                StorageType = TeensyStorageType.SD
            });

            await Task.Delay(3000);

            r = await f.Client.GetAsync<LaunchFileEndpoint, LaunchFileRequest, LaunchFileResponse>(new LaunchFileRequest
            {
                DeviceId = deviceId,
                FilePath = "/games/Large/706k The Secret of Monkey Island (D42) [EasyFlash].crt",
                StorageType = TeensyStorageType.SD
            });

            await Task.Delay(3000);

            r = await f.Client.GetAsync<LaunchFileEndpoint, LaunchFileRequest, LaunchFileResponse>(new LaunchFileRequest
            {
                DeviceId = deviceId,
                FilePath = "/games/Large/738k Elvira II - The Jaws of Cerberus (by $olo1870) [EasyFlash].crt",
                StorageType = TeensyStorageType.SD
            });

            await Task.Delay(3000);

            r = await f.Client.GetAsync<LaunchFileEndpoint, LaunchFileRequest, LaunchFileResponse>(new LaunchFileRequest
            {
                DeviceId = deviceId,
                FilePath = "/music/MUSICIANS/L/LukHash/Alpha.sid",
                StorageType = TeensyStorageType.SD
            });

            await Task.Delay(3000);

            r = await f.Client.GetAsync<LaunchFileEndpoint, LaunchFileRequest, LaunchFileResponse>(new LaunchFileRequest
            {
                DeviceId = deviceId,
                FilePath = "/music/MUSICIANS/J/Jammic/Wasted_Years.sid",
                StorageType = TeensyStorageType.SD
            });

            await Task.Delay(3000);

            r = await f.Client.GetAsync<LaunchFileEndpoint, LaunchFileRequest, LaunchFileResponse>(new LaunchFileRequest
            {
                DeviceId = deviceId,
                FilePath = "/games/Large/738k Elvira II - The Jaws of Cerberus (by $olo1870) [EasyFlash].crt",
                StorageType = TeensyStorageType.SD
            });

            r = await f.Client.GetAsync<LaunchFileEndpoint, LaunchFileRequest, LaunchFileResponse>(new LaunchFileRequest
            {
                DeviceId = deviceId,
                FilePath = "/music/MUSICIANS/J/Jammic/Wasted_Years.sid",
                StorageType = TeensyStorageType.SD
            });

            // Assert  
            r.Should().BeSuccessful<LaunchFileResponse>()
                .WithStatusCode(HttpStatusCode.OK)
                .WithContentNotNull();

            r.Content.Message.Should().Contain("Success");
        }

        [Fact]
        public async void When_LaunchCalled_WithInvalidPath_ReturnsNotFound()
        {
            // Arrange              
            var deviceId = await f.ConnectToFirstDevice();

            // Act  
            var request = new LaunchFileRequest
            {
                DeviceId = deviceId,
                FilePath = NonExistentPath,
                StorageType = TeensyStorageType.SD
            };

            var r = await f.Client.GetAsync<LaunchFileEndpoint, LaunchFileRequest, ProblemDetails>(request);
            // Assert  
            r.Should().BeProblem()
                .WithStatusCode(HttpStatusCode.NotFound);
        }

        [Fact]
        public async void When_LaunchCalled_WithInvalidDeviceId_ReturnsNotFound()
        {
            // Act  
            var request = new LaunchFileRequest
            {
                DeviceId = "invalid-device-id",
                FilePath = NonExistentPath,
                StorageType = TeensyStorageType.SD
            };
            var r = await f.Client.GetAsync<LaunchFileEndpoint, LaunchFileRequest, ValidationProblemDetails>(request);

            // Assert  
            r.Should().BeValidationProblem()
                .WithStatusCode(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async void When_LaunchCalled_WithDeviceThatDoesntExist_ReturnsNotFound()
        {
            // Act  
            var request = new LaunchFileRequest
            {
                DeviceId = Guid.NewGuid().ToString().GenerateFilenameSafeHash(),
                FilePath = NonExistentPath
            };
            var r = await f.Client.GetAsync<LaunchFileEndpoint, LaunchFileRequest, ProblemDetails>(request);

            // Assert  
            r.Should().BeProblem()
                .WithStatusCode(HttpStatusCode.NotFound);
        }

        [Fact]
        public async void When_LaunchCalled_WithInvalidStorageType_ReturnsBadRequest()
        {
            // Act  
            var request = new LaunchFileRequest
            {
                DeviceId = "invalid-device-id",
                FilePath = NonExistentPath,
                StorageType = (TeensyStorageType)999
            };
            var r = await f.Client.GetAsync<LaunchFileEndpoint, LaunchFileRequest, ProblemDetails>(request);

            // Assert  
            r.Should().BeProblem()
                .WithStatusCode(HttpStatusCode.BadRequest);
        }

        public void Dispose() => f.Reset();
    }
}