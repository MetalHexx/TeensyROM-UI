using FluentAssertions;
using TeensyRom.Api.Endpoints.FindCarts;
using TeensyRom.Api.Endpoints.ConnectDevice;
using TeensyRom.Api.Endpoints.ClosePort;
using TeensyRom.Api.Models;

namespace TeensyRom.Api.Tests.Integration
{
    [Collection("Endpoint")]
    public class ConnectDeviceTests(EndpointFixture f) : IDisposable
    {
        [Fact]
        public async void When_Called_ResponseSuccessful()
        {
            // Arrange
            var devices = await f.Client.GetAsync<FindDevicesEndpoint, FindDevicesRequest, FindDevicesResponse>(new FindDevicesRequest 
            {
                AutoConnectNew = false
            });
            var expectedDeviceId = devices.Content.Devices.First().DeviceId;

            // Act
            var r = await f.Client.PostAsync<ConnectDeviceEndpoint, ConnectDeviceRequest, ConnectDeviceResponse>(new ConnectDeviceRequest
            {
                DeviceId = expectedDeviceId
            });

            var finalDevices = await f.Client.GetAsync<FindDevicesEndpoint, FindDevicesRequest, FindDevicesResponse>(new FindDevicesRequest 
            {
                AutoConnectNew = false
            });

            // Assert
            r.Should().BeSuccessful<ConnectDeviceResponse>()
                .WithStatusCode(HttpStatusCode.OK)
                .WithContentNotNull();

            r.Content.Should().NotBeNull();
            r.Content.ConnectedCart.Should().NotBeNull();
            r.Content.ConnectedCart.DeviceId.Should().Be(expectedDeviceId);            
            finalDevices.Content.Devices.Should().ContainSingle(d => d.DeviceId == expectedDeviceId && d.IsConnected);
            finalDevices.Content.Devices.Where(d => d.IsConnected).Count().Should().Be(1);
            r.Content.Message.Should().Contain("Connection successful!");
        }

        public void Dispose() => f.Reset();
    }
}