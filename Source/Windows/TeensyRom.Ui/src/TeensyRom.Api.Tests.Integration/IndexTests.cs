using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Api.Endpoints.Files.Index;
using TeensyRom.Api.Endpoints.FindCarts;
using TeensyRom.Api.Endpoints.ConnectDevice;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Device;
using TeensyRom.Core.Entities.Storage;
using System.Diagnostics;

namespace TeensyRom.Api.Tests.Integration
{

    [Collection("Endpoint")]
    public class IndexTests(EndpointFixture f) : IDisposable
    {
        [Fact]
        public async Task When_Indexing_WithoutPath_SuccessReturned()
        {
            // Arrange
            var deviceId = await f.GetConnectedDevice();
            f.DeleteCache(deviceId, TeensyStorageType.SD);

            // Act - TrClient automatically handles enum serialization
            var request = new IndexRequest
            {
                DeviceId = deviceId,
                StorageType = TeensyStorageType.SD,
                StartingPath = null
            };
            var response = await f.Client.PostAsync<IndexEndpoint, IndexRequest, IndexResponse>(request);

            // Assert
            response.Should().BeSuccessful<IndexResponse>()
                .WithStatusCode(HttpStatusCode.OK)
                .WithContentNotNull();

            f.CacheExists(deviceId, TeensyStorageType.SD).Should().BeTrue();
            response.Content.Message.Should().Contain("Success");

            //foreach (var item in Enumerable.Range(0, 100))
            //{
            //    // Act
            //    Debug.WriteLine($"*********************************Iteration {item}*********************************");
            //    Debug.WriteLine($"*********************************Iteration {item}*********************************");
            //    Debug.WriteLine($"*********************************Iteration {item}*********************************");
            //    Debug.WriteLine($"*********************************Iteration {item}*********************************");
            //    Debug.WriteLine($"*********************************Iteration {item}*********************************");
            //    Debug.WriteLine($"*********************************Iteration {item}*********************************");
            //    Debug.WriteLine($"*********************************Iteration {item}*********************************");
            //    Debug.WriteLine($"*********************************Iteration {item}*********************************");
            //    Debug.WriteLine($"*********************************Iteration {item}*********************************");
            //    Debug.WriteLine($"*********************************Iteration {item}*********************************");
            //    Debug.WriteLine($"*********************************Iteration {item}*********************************");

                

            //}
        }

        [Fact]
        public async Task When_Indexing_WithGamePath_SuccessReturned()
        {
            // Arrange
            var deviceId = await f.GetConnectedDevice();
            f.DeleteCache(deviceId!, TeensyStorageType.SD);

            // Act - TrClient automatically handles enum serialization
            var request = new IndexRequest
            {
                DeviceId = deviceId,
                StorageType = TeensyStorageType.SD,
                StartingPath = "/games"
            };
            var response = await f.Client.PostAsync<IndexEndpoint, IndexRequest, IndexResponse>(request);

            // Assert
            response.Should().BeSuccessful<IndexResponse>()
                .WithStatusCode(HttpStatusCode.OK)
                .WithContentNotNull();

            f.CacheExists(deviceId, TeensyStorageType.SD).Should().BeTrue();
            response.Content.Message.Should().Contain("Success");

            //foreach (var item in Enumerable.Range(0, 100))
            //{
            //    Debug.WriteLine($"***********************************");
            //    Debug.WriteLine($"************Iteration {item}************");
            //    Debug.WriteLine($"***********************************");


            //}
        }

        [Fact]
        public async Task When_Indexing_WithBadPath_BadRequestReturned()
        {
            // Arrange - TrClient automatically handles enum serialization and proper request signature
            var availableDevices = await f.Client.GetAsync<FindDevicesEndpoint, FindDevicesRequest, FindDevicesResponse>(new FindDevicesRequest());
            var deviceId = availableDevices.Content.Devices.First().DeviceId;

            // Act - TrClient automatically handles enum serialization
            var request = new IndexRequest
            {
                DeviceId = deviceId,
                StorageType = TeensyStorageType.SD,
                StartingPath = "#(*&_#_()*&$"
            };
            var response = await f.Client.PostAsync<IndexEndpoint, IndexRequest, ValidationProblemDetails>(request);

            // Assert
            response.Should()
                .BeValidationProblem()
                .WithKeyAndValue("StartingPath", "Path must be a valid Unix path.");
        }

        [Fact]
        public async Task When_Indexing_WithBadDeviceId_BadRequestReturned()
        {
            // Act - TrClient automatically handles enum serialization
            var request = new IndexRequest
            {
                DeviceId = "12345!!&^#",
                StorageType = TeensyStorageType.SD,
                StartingPath = "/"
            };
            var response = await f.Client.PostAsync<IndexEndpoint, IndexRequest, ValidationProblemDetails>(request);

            // Assert
            response.Should()
                .BeValidationProblem()
                .WithKeyAndValue("DeviceId", "Invalid Device Id.");
        }


        public void Dispose() => f.Reset();
    }
}
