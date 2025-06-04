using TeensyRom.Api.Endpoints.ClosePort;
using TeensyRom.Api.Endpoints.FindCarts;
using TeensyRom.Api.Endpoints.ConnectDevice;

namespace TeensyRom.Api.Tests.Integration
{
    [Collection("Endpoint")]
    public class FindDevicesTests(EndpointFixture f) :IDisposable

    {
        [Fact]
        public async Task Given_NoDevicesConnected_When_Called_And_AutoConnectNew_false_NoDevicesConnected()
        {
            // Act
            var r = await f.Client.GetAsync<FindDevicesEndpoint, FindDevicesRequest, FindDevicesResponse>(new FindDevicesRequest 
            {
                AutoConnectNew = false
            });

            // Assert
            r.Should()
                .BeSuccessful<FindDevicesResponse>()
                .WithStatusCode(HttpStatusCode.OK)
                .WithContentNotNull();

            r.Content.Message.Should().Be("Success!");
            r.Content.Devices.All(d => d.IsConnected).Should().BeFalse();
            r.Content.Devices.Count.Should().Be(2);
        }

        [Fact]
        public async Task Given_NoDevicesConnected_When_Called_And_AutoConnectNew_True_AllDevicesConnected()
        {   
            // Act
            var r = await f.Client.GetAsync<FindDevicesEndpoint, FindDevicesRequest, FindDevicesResponse>(new FindDevicesRequest 
            { 
                AutoConnectNew = true 
            });

            // Assert
            r.Should().BeSuccessful<FindDevicesResponse>()
                .WithStatusCode(HttpStatusCode.OK)
                .WithContentNotNull();
            r.Content.Devices.Should().NotBeNullOrEmpty();
            r.Content.Devices.Count.Should().BeGreaterThan(0);
            r.Content.Devices.All(d => d.IsConnected).Should().BeTrue();
        }

        [Fact]
        public async Task Given_DeviceConnected_When_Called_And_AutoConnectNew_False_Then_Existing_DeviceRemainsConnected()
        {
            // Arrange
            var deviceId = await f.ConnectToDevices();
            var expectedDisconnectedDevice = deviceId.First().DeviceId;
            await f.DisconnectDevice(expectedDisconnectedDevice);
            var expectedConnectedDevice = deviceId.First(d => d.DeviceId != expectedDisconnectedDevice);

            // Act
            var r = await f.Client.GetAsync<FindDevicesEndpoint, FindDevicesRequest, FindDevicesResponse>(new FindDevicesRequest 
            { 
                AutoConnectNew = false 
            });

            // Assert
            r.Should().BeSuccessful<FindDevicesResponse>()
                .WithStatusCode(HttpStatusCode.OK)
                .WithContentNotNull();

            r.Content.Devices.Count.Should().Be(2);
            r.Content.Devices.Should().ContainSingle(d => d.DeviceId == expectedConnectedDevice.DeviceId && d.IsConnected);
            r.Content.Devices.Should().ContainSingle(d => d.DeviceId == expectedDisconnectedDevice && !d.IsConnected);
        }

        public void Dispose() => f.Reset();
    }
}