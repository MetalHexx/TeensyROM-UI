using TeensyRom.Api.Endpoints.ClosePort;
using TeensyRom.Api.Endpoints.FindCarts;
using TeensyRom.Api.Endpoints.ConnectDevice;
using TeensyRom.Core.Common;

namespace TeensyRom.Api.Tests.Integration
{
    [Collection("Endpoint")]
    public class DisconnectDeviceTests(EndpointFixture f) : IDisposable
    {
        [Fact]
        public async Task When_Closing_ValidDevice_ResponseSuccessful()
        {
            // Arrange
            var deviceId = await f.ConnectToFirstDevice();

            // Act
            var closeRequest = new DisconnectDeviceRequest { DeviceId = deviceId };
            var r = await f.Client.DeleteAsync<DisconnectDeviceEndpoint, DisconnectDeviceRequest, DisconnectDeviceResponse>(closeRequest);

            var findResponse = await f.Client.GetAsync<FindDevicesEndpoint, FindDevicesResponse>();

            // Assert
            r.Should().BeSuccessful<DisconnectDeviceResponse>()
                .WithStatusCode(HttpStatusCode.OK)
                .WithContentNotNull();

            findResponse.Content.ConnectedCarts.Count.Should().Be(0);

            r.Content.Message.Should().Be("Success!");
        }

        [Fact]
        public async Task When_Closing_WithInvalidFormatDeviceId_ReturnsBadRequest()
        {
            // Act
            var closeRequest = new DisconnectDeviceRequest { DeviceId = "!!!BAD!!!" };
            var r = await f.Client.DeleteAsync<DisconnectDeviceEndpoint, DisconnectDeviceRequest, ValidationProblemDetails>(closeRequest);

            // Assert
            r.Should().BeValidationProblem()
                .WithStatusCode(HttpStatusCode.BadRequest);

            r.Content.Errors.Should().ContainKey("DeviceId");
            r.Content.Errors["DeviceId"].First().Should().Be("Device ID must be a valid filename-safe hash of 8 characters long.");
        }

        [Fact]
        public async Task When_Closing_UnknownDeviceId_ReturnsNotFound()
        {
            // Act
            var deviceId = Guid.NewGuid().ToString().GenerateFilenameSafeHash();
            var closeRequest = new DisconnectDeviceRequest { DeviceId = deviceId };
            var r = await f.Client.DeleteAsync<DisconnectDeviceEndpoint, DisconnectDeviceRequest, ProblemDetails>(closeRequest);

            // Assert
            r.Should().BeProblem()
                .WithStatusCode(HttpStatusCode.NotFound);

            r.Content.Title.Should().Contain($"The device {deviceId} was not found.");
        }

        public void Dispose() => f.Reset();
    }

}
