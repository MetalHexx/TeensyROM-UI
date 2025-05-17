using System.Net;
using TeensyRom.Api.Endpoints.FindCarts;
using TeensyRom.Api.Endpoints.OpenPort;
using TeensyRom.Api.Endpoints.ResetDevice;
using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Api.Tests.Integration
{
    [Collection("Endpoint")]
    public class ResetDeviceTests(EndpointFixture f) : IDisposable
    {
        [Fact]
        public async Task When_Resetting_ValidDevice_ResponseSuccessful()
        {
            // Arrange
            var findCartsResponse = await f.Client.GetAsync<FindCartsEndpoint, FindCartsResponse>();
            var deviceId = findCartsResponse.Content.AvailableCarts.First().DeviceId;

            // Ensure the port is opened
            var openRequest = new OpenPortRequest { DeviceId = deviceId };
            await f.Client.GetAsync<OpenPortEndpoint, OpenPortRequest, OpenPortResponse>(openRequest);

            // Act
            var resetRequest = new ResetDeviceRequest { DeviceId = deviceId };
            var r = await f.Client.PutAsync<ResetDeviceEndpoint, ResetDeviceRequest, ResetDeviceResponse>(resetRequest);

            // Assert
            r.Should().BeSuccessful<ResetDeviceResponse>()
                .WithStatusCode(HttpStatusCode.OK)
                .WithContentNotNull();
        }

        [Fact]
        public async Task When_Resetting_WithInvalidDeviceId_ReturnsBadRequest()
        {
            // Act
            var resetRequest = new ResetDeviceRequest { DeviceId = "invalid-device-id" };
            var r = await f.Client.PutAsync<ResetDeviceEndpoint, ResetDeviceRequest, ProblemDetails>(resetRequest);

            // Assert
            r.Should().BeProblem()
                .WithStatusCode(HttpStatusCode.BadRequest);

            r.Content.Title.Should().Contain("The device invalid-device-id was not found.");
        }

        public void Dispose() => f.Reset();
    }
}
