using FluentAssertions;
using TeensyRom.Api.Endpoints.FindCarts;
using TeensyRom.Api.Endpoints.ConnectDevice;

namespace TeensyRom.Api.Tests.Integration
{
    [Collection("Endpoint")]
    public class ConnectDeviceTests(EndpointFixture f) : IDisposable
    {
        [Fact]
        public async void When_Called_ResponseSuccessful()
        {
            // Arrange
            var devices = await f.Client.GetAsync<FindDevicesEndpoint, FindDevicesResponse>();

            // Act
            var openRequest = new ConnectDeviceRequest
            {
                DeviceId = devices.Content.Devices.First().DeviceId
            };
            var r = await f.Client.PostAsync<ConnectDeviceEndpoint, ConnectDeviceRequest, ConnectDeviceResponse>(openRequest);

            // Assert
            r.Should().BeSuccessful<ConnectDeviceResponse>()
                .WithStatusCode(HttpStatusCode.OK)
                .WithContentNotNull();

            r.Content.Should().NotBeNull();
            r.Content.ConnectedCart.Should().NotBeNull();
            r.Content.ConnectedCart.DeviceId.Should().NotBeNullOrEmpty();
            r.Content.ConnectedCart.DeviceId.Should().Be(openRequest.DeviceId);
            r.Content.ConnectedCart.ComPort.Should().NotBeNullOrEmpty();            

            r.Content.Message.Should().Contain("Connection successful!");
        }

        public void Dispose() => f.Reset();
    }
}