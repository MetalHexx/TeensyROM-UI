using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Api.Endpoints.Files.Index;
using TeensyRom.Api.Endpoints.Files.LaunchFile;
using TeensyRom.Api.Endpoints.FindCarts;
using TeensyRom.Api.Endpoints.OpenPort;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Device;
using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Api.Tests.Integration
{

    [Collection("Endpoint")]
    public class IndexTests(EndpointFixture f) : IDisposable
    {
        [Fact]
        public async Task When_Indexing_WithoutPath_SuccessReturned()
        {
            // Arrange
            var deviceResult = await f.Client.GetAsync<FindCartsEndpoint, FindCartsResponse>();
            var deviceId = deviceResult.Content.AvailableCarts.First().DeviceId;
            var openPortRequest = new OpenPortRequest
            {
                DeviceId = deviceId!
            };
            var openPortResponse = await f.Client.GetAsync<OpenPortEndpoint, OpenPortRequest, OpenPortResponse>(openPortRequest);
            DeleteCache(deviceId!, TeensyStorageType.SD);

            // Act
            var request = new IndexRequest
            {
                DeviceId = deviceId!,
                StorageType = TeensyStorageType.SD,
                Path = null
            };
            var response = await f.Client.PostAsync<IndexEndpoint, IndexRequest, IndexResponse>(request);

            // Assert
            response.Should().BeSuccessful<IndexResponse>()
                .WithStatusCode(HttpStatusCode.OK)
                .WithContentNotNull();

            CacheExists(deviceId!, TeensyStorageType.SD).Should().BeTrue();
            response.Content.Message.Should().Contain("Success");
        }

        [Fact]
        public async Task When_Indexing_WithGamePath_SuccessReturned()
        {
            // Arrange
            var deviceResult = await f.Client.GetAsync<FindCartsEndpoint, FindCartsResponse>();
            var deviceId = deviceResult.Content.AvailableCarts.First().DeviceId;
            var openPortRequest = new OpenPortRequest
            {
                DeviceId = deviceId!
            };
            var openPortResponse = await f.Client.GetAsync<OpenPortEndpoint, OpenPortRequest, OpenPortResponse>(openPortRequest);
            DeleteCache(deviceId!, TeensyStorageType.SD);

            // Act
            var request = new IndexRequest
            {
                DeviceId = deviceId!,
                StorageType = TeensyStorageType.SD,
                Path = "/games"
            };
            var response = await f.Client.PostAsync<IndexEndpoint, IndexRequest, IndexResponse>(request);

            // Assert
            response.Should().BeSuccessful<IndexResponse>()
                .WithStatusCode(HttpStatusCode.OK)
                .WithContentNotNull();

            CacheExists(deviceId!, TeensyStorageType.SD).Should().BeTrue();
            response.Content.Message.Should().Contain("Success");
        }

        [Fact]
        public async Task When_Indexing_WithBadPath_BadRequestReturned()
        {
            // Arrange
            var availableDevices = await f.Client.GetAsync<FindCartsEndpoint, FindCartsResponse>();
            var deviceId = availableDevices.Content.AvailableCarts.First().DeviceId;

            // Act
            var request = new IndexRequest
            {
                DeviceId = deviceId,
                StorageType = TeensyStorageType.SD,
                Path = "#(*&_#_()*&$"
            };
            var response = await f.Client.PostAsync<IndexEndpoint, IndexRequest, ValidationProblemDetails>(request);

            // Assert
            response.Should()
                .BeValidationProblem()
                .WithKeyAndValue("Path", "Path must be a valid Unix path.");
        }

        [Fact]
        public async Task When_Indexing_WithoutDeviceId_BadRequestReturned()
        {
            // Act
            var request = new IndexRequest
            {
                StorageType = TeensyStorageType.SD,
                Path = "/"
            };
            var response = await f.Client.PostAsync<IndexEndpoint, IndexRequest, ValidationProblemDetails>(request);

            // Assert
            response.Should()
                .BeValidationProblem()
                .WithKeyAndValue("DeviceId", "Invalid Device Id.");
        }

        [Fact]
        public async Task When_Indexing_WithBadDeviceId_BadRequestReturned()
        {
            // Act
            var request = new IndexRequest
            {
                DeviceId = "12345!!&^#",
                StorageType = TeensyStorageType.SD,
                Path = "/"
            };
            var response = await f.Client.PostAsync<IndexEndpoint, IndexRequest, ValidationProblemDetails>(request);

            // Assert
            response.Should()
                .BeValidationProblem()
                .WithKeyAndValue("DeviceId", "Invalid Device Id.");
        }

        private string DeleteCache(string deviceId, TeensyStorageType storageType)
        {
            var path = GetCachePath(deviceId, storageType);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            return path;
        }

        private bool CacheExists(string deviceId, TeensyStorageType storageType)
        {
            var path = GetCachePath(deviceId, storageType);
            return File.Exists(path);
        }

        private string GetCachePath(string deviceId, TeensyStorageType storageType)
        {
            var path = string.Empty;

            if (storageType == TeensyStorageType.SD)
            {
                path = Path.Combine(
                    Assembly.GetExecutingAssembly().GetPath(),
                    StorageHelper.Sd_Cache_File_Relative_Path,
                    $"{StorageHelper.Sd_Cache_File_Name}{deviceId}{StorageHelper.Cache_File_Extension}");
            }
            else
            {
                path = Path.Combine(
                    Assembly.GetExecutingAssembly().GetPath(),
                    StorageHelper.Usb_Cache_File_Relative_Path,
                    $"{StorageHelper.Usb_Cache_File_Name}{deviceId}{StorageHelper.Cache_File_Extension}");
            }
            return path;
        }

        public void Dispose() => f.Reset();
    }
}
