using TeensyRom.Api.Endpoints.ClosePort;
using TeensyRom.Api.Endpoints.FindCarts;
using TeensyRom.Api.Endpoints.ConnectDevice;

namespace TeensyRom.Api.Tests.Integration
{
    [Collection("Endpoint")]
    public class FindDevicesTests(EndpointFixture f) :IDisposable

    {
        [Fact]
        public async void When_Called_AvailableCartsReturned()
        {
            // Act
            var r = await f.Client.GetAsync<FindDevicesEndpoint, FindDevicesResponse>();

            // Assert
            r.Should()
                .BeSuccessful<FindDevicesResponse>()
                .WithStatusCode(HttpStatusCode.OK)
                .WithContentNotNull();

            r.Content.Message.Should().Be("Success!");
            r.Content.AvailableCarts.Should().NotBeNullOrEmpty();            
            r.Content.ConnectedCarts.Should().BeEmpty();
        }

        [Fact]
        public async void Given_CartWasOpened_When_FindCalled_ConnectedCartsReturned()
        {
            // Arrange
            var initialCarts = await f.Client.GetAsync<FindDevicesEndpoint, FindDevicesResponse>();
            var expectedConnectedCart = initialCarts.Content.AvailableCarts.First();
            var expectedAvailableCount = initialCarts.Content.AvailableCarts.Count;
            var openRequest = new ConnectDeviceRequest
            {
                DeviceId = expectedConnectedCart.DeviceId
            };
            var openResponse = await f.Client.PostAsync<ConnectDeviceEndpoint, Endpoints.ConnectDevice.ConnectDeviceRequest, ConnectDeviceResponse>(openRequest);

            // Act
            var r = await f.Client.GetAsync<FindDevicesEndpoint, FindDevicesResponse>();

            // Assert
            r.Should().BeSuccessful<FindDevicesResponse>()
                .WithStatusCode(HttpStatusCode.OK)
                .WithContentNotNull();

            r.Content.AvailableCarts.Should().NotBeNullOrEmpty();
            r.Content.AvailableCarts.Count.Should().Be(expectedAvailableCount);
            r.Content.ConnectedCarts.Count.Should().Be(1);
            r.Content.ConnectedCarts.First().DeviceId.Should().Be(expectedConnectedCart.DeviceId);
        }

        public void Dispose() => f.Reset();
    }
}