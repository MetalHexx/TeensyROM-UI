using System.Reflection;
using TeensyRom.Api.Endpoints.Files.Index;
using TeensyRom.Api.Endpoints.Files.IndexAll;
using TeensyRom.Api.Endpoints.FindCarts;
using TeensyRom.Api.Endpoints.OpenPort;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Api.Tests.Integration
{
    [Collection("Endpoint")]
    public class IndexAllTests(EndpointFixture f)
    {
        [Fact]
        public async Task When_IndexingAll_SuccessReturned()
        {
            // Arrange
            var deviceResult = await RadTestClientExtensions.GetAsync<FindCartsEndpoint, FindCartsResponse>(f.Client);

            foreach (var item in deviceResult.Content.AvailableCarts)
            {
                await f.Client.GetAsync<OpenPortEndpoint, OpenPortRequest, OpenPortResponse>(new OpenPortRequest
                {
                    DeviceId = item.DeviceId!
                });
                DeleteCache(item.DeviceId!, TeensyStorageType.SD);
                DeleteCache(item.DeviceId!, TeensyStorageType.USB);
            }

            // Act
            var response = await f.Client.PostAsync<IndexAllEndpoint, IndexResponse>();

            // Assert
            response.Should().BeSuccessful<IndexResponse>()
                .WithStatusCode(HttpStatusCode.OK)
                .WithContentNotNull();

            deviceResult.Content.AvailableCarts
                .Where(d => d.SdStorage.Available)
                .Should()
                .AllSatisfy(item =>
                {
                    CacheExists(item.DeviceId!, TeensyStorageType.SD).Should().BeTrue();
                });

            deviceResult.Content.AvailableCarts
                .Where(d => d.UsbStorage.Available)
                .Should()
                .AllSatisfy(item =>
                {
                    CacheExists(item.DeviceId!, TeensyStorageType.USB).Should().BeTrue();
                });
            response.Content.Message.Should().Contain("Success");
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
                    StorageConstants.Sd_Cache_File_Relative_Path,
                    $"{StorageConstants.Sd_Cache_File_Name}{deviceId}{StorageConstants.Cache_File_Extension}");
            }
            else
            {
                path = Path.Combine(
                    Assembly.GetExecutingAssembly().GetPath(),
                    StorageConstants.Usb_Cache_File_Relative_Path,
                    $"{StorageConstants.Usb_Cache_File_Name}{deviceId}{StorageConstants.Cache_File_Extension}");
            }
            return path;
        }
    }
}
