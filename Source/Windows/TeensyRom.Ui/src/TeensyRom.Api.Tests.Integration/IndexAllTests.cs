using System.Reflection;
using TeensyRom.Api.Endpoints.Files.Index;
using TeensyRom.Api.Endpoints.Files.IndexAll;
using TeensyRom.Api.Endpoints.FindCarts;
using TeensyRom.Api.Endpoints.ConnectDevice;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Api.Tests.Integration
{
    [Collection("Endpoint")]
    public class IndexAllTests(EndpointFixture f) : IDisposable
    {
        [Fact]
        public async Task When_IndexingAll_SuccessReturned()
        {
            // Arrange
            var deviceResult = await f.Client.GetAsync<FindDevicesEndpoint, FindDevicesResponse>();

            foreach (var item in deviceResult.Content.AvailableCarts)
            {
                await f.Client.PostAsync<ConnectDeviceEndpoint, ConnectDeviceRequest, ConnectDeviceResponse>(new ConnectDeviceRequest
                {
                    DeviceId = item.DeviceId!
                });
                f.DeleteCache(item.DeviceId!, TeensyStorageType.SD);
                f.DeleteCache(item.DeviceId!, TeensyStorageType.USB);
            }

            // Act
            var response = await f.Client.PostAsync<IndexAllEndpoint, IndexResponse>();

            // Assert
            response.Should().BeSuccessful<IndexResponse>()
                .WithStatusCode(HttpStatusCode.OK)
                .WithContentNotNull();

            var availableSdDevices = deviceResult.Content.AvailableCarts
                .Where(d => d.SdStorage.Available)
                .ToList();

            if (availableSdDevices.Count > 0) 
            {
                availableSdDevices
                .Should()
                .AllSatisfy(item =>
                {
                    f.CacheExists(item.DeviceId!, TeensyStorageType.SD).Should().BeTrue();
                });
            }

            var availableUsbDevices = deviceResult.Content.AvailableCarts
                    .Where(d => d.UsbStorage.Available)
                    .ToList();

            if (availableUsbDevices.Count > 0)
            {
                availableUsbDevices
                    .Should()
                    .AllSatisfy(item =>
                    {
                        f.CacheExists(item.DeviceId!, TeensyStorageType.USB).Should().BeTrue();
                    });
                response.Content.Message.Should().Contain("Success");
            }
        }
        public void Dispose() => f.Reset();
    }
}
