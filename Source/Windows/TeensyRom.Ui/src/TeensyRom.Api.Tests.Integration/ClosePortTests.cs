using TeensyRom.Api.Endpoints.ClosePort;
using TeensyRom.Api.Endpoints.FindCarts;
using TeensyRom.Api.Endpoints.OpenPort;
using TeensyRom.Core.Common;

namespace TeensyRom.Api.Tests.Integration
{
    [Collection("Endpoint")]
    public class ClosePortTests(EndpointFixture f)
    {
        [Fact]
        public async Task When_Closing_ValidDevice_ResponseSuccessful()
        {
            // Arrange
            var findResponse = await f.Client.GetAsync<FindCartsEndpoint, FindCartsResponse>();
            var deviceId = findResponse.Content.AvailableCarts.First().DeviceId;

            await f.Client.GetAsync<OpenPortEndpoint, OpenPortRequest, OpenPortResponse>(
                new OpenPortRequest { DeviceId = deviceId });

            // Act
            var closeRequest = new ClosePortRequest { DeviceId = deviceId };
            var r = await f.Client.DeleteAsync<ClosePortEndpoint, ClosePortRequest, ClosePortResponse>(closeRequest);

            // Assert
            r.Should().BeSuccessful<ClosePortResponse>()
                .WithStatusCode(HttpStatusCode.OK)
                .WithContentNotNull();

            r.Content.Message.Should().Be("Success!");
        }

        [Fact]
        public async Task When_Closing_WithInvalidFormatDeviceId_ReturnsBadRequest()
        {
            // Act
            var closeRequest = new ClosePortRequest { DeviceId = "!!!BAD!!!" };
            var r = await f.Client.DeleteAsync<ClosePortEndpoint, ClosePortRequest, ValidationProblemDetails>(closeRequest);

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
            var closeRequest = new ClosePortRequest { DeviceId = deviceId };
            var r = await f.Client.DeleteAsync<ClosePortEndpoint, ClosePortRequest, ProblemDetails>(closeRequest);

            // Assert
            r.Should().BeProblem()
                .WithStatusCode(HttpStatusCode.NotFound);

            r.Content.Title.Should().Contain($"The device {deviceId} was not found.");
        }
    }

}
