using FluentAssertions;
using TeensyRom.Api.Endpoints.Files.LaunchFile;
using TeensyRom.Api.Endpoints.FindCarts;
using TeensyRom.Api.Endpoints.OpenPort;

namespace TeensyRom.Api.Tests.Integration
{
    [Collection("Endpoint")]
    public class LaunchFileTests(EndpointFixture f)
    {
        [Fact]
        public async void Given_DeviceConnected_When_LaunchCalled_WithValidPath_ReturnsSuccess()
        {
            // Arrange              
            var availableDevices = await RadTestClientExtensions.GetAsync<FindCartsEndpoint, FindCartsResponse>(f.Client);
            var deviceId = availableDevices.Content.AvailableCarts.First().DeviceId;
            var openPortRequest = new OpenPortRequest { DeviceId = deviceId };

            await RadTestClientExtensions.GetAsync<OpenPortEndpoint, OpenPortRequest, OpenPortResponse>(f.Client, openPortRequest);

            // Act  
            var request = new LaunchFileRequest
            {
                DeviceId = deviceId,
                Path = "/music/MUSICIANS/L/LukHash/Alpha.sid"
            };
            var r = await RadTestClientExtensions.GetAsync<LaunchFileEndpoint, LaunchFileRequest, LaunchFileResponse>(f.Client, request);

            // Assert  
            r.Should().BeSuccessful<LaunchFileResponse>()
                .WithStatusCode(HttpStatusCode.OK)
                .WithContentNotNull();

            r.Content.Message.Should().Contain("Success");
        }

        [Fact]
        public async void When_2_LargeFileLaunched_WithValidPath_ReturnsSuccess()
        {
            // Arrange              
            var availableDevices = await RadTestClientExtensions.GetAsync<FindCartsEndpoint, FindCartsResponse>(f.Client);
            var deviceId = availableDevices.Content.AvailableCarts.First().DeviceId;
            var openPortRequest = new OpenPortRequest { DeviceId = deviceId };

            await RadTestClientExtensions.GetAsync<OpenPortEndpoint, OpenPortRequest, OpenPortResponse>(f.Client, openPortRequest);

            // Act  
            var request = new LaunchFileRequest
            {
                DeviceId = deviceId,
                Path = "/games/Large/706k The Secret of Monkey Island (D42) [EasyFlash].crt"
            };
            var r = await RadTestClientExtensions.GetAsync<LaunchFileEndpoint, LaunchFileRequest, LaunchFileResponse>(f.Client, request);

            request = new LaunchFileRequest
            {
                DeviceId = deviceId,
                Path = "/games/Large/634k Gauntlet III - Final Quest [EasyFlash].crt"
            };
            r = await RadTestClientExtensions.GetAsync<LaunchFileEndpoint, LaunchFileRequest, LaunchFileResponse>(f.Client, request);

            request = new LaunchFileRequest
            {
                DeviceId = deviceId,
                Path = "/games/Large/738k Elvira II - The Jaws of Cerberus (by $olo1870) [EasyFlash].crt"
            };
            r = await RadTestClientExtensions.GetAsync<LaunchFileEndpoint, LaunchFileRequest, LaunchFileResponse>(f.Client, request);

            // Assert  
            r.Should().BeSuccessful<LaunchFileResponse>()
                .WithStatusCode(HttpStatusCode.OK)
                .WithContentNotNull();

            r.Content.Message.Should().Contain("Success");
        }
    }
}