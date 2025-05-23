using FluentAssertions;
using TeensyRom.Api.Endpoints.ClosePort;
using TeensyRom.Api.Endpoints.Files.LaunchFile;
using TeensyRom.Api.Endpoints.FindCarts;
using TeensyRom.Api.Endpoints.ConnectDevice;
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
        private const string IncompatibleSid = "/music/MUSICIANS/J/Jammer/Immigrant_Song.sid";

        [Fact]
        public async Task When_LaunchingIncompatibleSid_ReturnsBadRequest()
        {
            // Arrange              
            var deviceId = await f.ConnectToFirstDevice();

            // Act  
            var request = new LaunchFileRequest
            {
                DeviceId = deviceId,
                FilePath = IncompatibleSid,
                StorageType = TeensyStorageType.SD
            };
            var r = await f.Client.PostAsync<LaunchFileEndpoint, LaunchFileRequest, ProblemDetails>(request);
            
            // Assert  
            r.Should().BeProblem()
                .WithStatusCode(HttpStatusCode.BadGateway);
        }

        [Fact]
        public async Task When_LaunchingVariousFilesInSequence_ReturnsSuccess()
        {
            // Arrange
            var deviceId = await f.ConnectToFirstDevice();

            List<string> filePaths =
            [
                "/music/MUSICIANS/L/LukHash/Alpha.sid",
                "/images/Dio2.kla",
                "/music/MUSICIANS/J/Jammic/Wasted_Years.sid",
                "/games/Large/706k The Secret of Monkey Island (D42) [EasyFlash].crt",
                "/music/MUSICIANS/E/Eclipse/True.sid",
                "/games/Large/738k Elvira II - The Jaws of Cerberus (by $olo1870) [EasyFlash].crt",
                "/music/MUSICIANS/M/Manganoid/Le_Shagma.sid",
                "/games/Very Large/A_Pig_Quest_1.02_ef.crt",
                "/games/Very Large/Lemmings [EasyFlash].crt",
                "/music/MUSICIANS/T/TDS/Under.sid"
            ];

            foreach (var filePath in filePaths)
            {
                var r = await f.Client.PostAsync<LaunchFileEndpoint, LaunchFileRequest, LaunchFileResponse>(new LaunchFileRequest
                {
                    DeviceId = deviceId,
                    FilePath = filePath,
                    StorageType = TeensyStorageType.SD
                });

                await Task.Delay(3000);

                // Assert after each
                r.Should().BeSuccessful<LaunchFileResponse>()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithContentNotNull();

                r.Content.Message.Should().Contain("Success");
            }
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

            var r = await f.Client.PostAsync<LaunchFileEndpoint, LaunchFileRequest, ProblemDetails>(request);
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
            var r = await f.Client.PostAsync<LaunchFileEndpoint, LaunchFileRequest, ValidationProblemDetails>(request);

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
            var r = await f.Client.PostAsync<LaunchFileEndpoint, LaunchFileRequest, ProblemDetails>(request);

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
            var r = await f.Client.PostAsync<LaunchFileEndpoint, LaunchFileRequest, ProblemDetails>(request);

            // Assert  
            r.Should().BeProblem()
                .WithStatusCode(HttpStatusCode.BadRequest);
        }

        public void Dispose() => f.Reset();
    }
}