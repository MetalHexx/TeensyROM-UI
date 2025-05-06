using FluentAssertions;
using TeensyRom.Api.Endpoints.FindCarts;
using TeensyRom.Api.Endpoints.OpenPort;

namespace TeensyRom.Api.Tests.Integration
{
    [Collection("Endpoint")]
    public class OpenPortTests(EndpointFixture f)

    {
        [Fact]
        public async void When_Called_ResponseSuccessful()
        {
            // Arrange
            var devices = await f.Client.GetAsync<FindCartsEndpoint, FindCartsResponse>();

            // Act
            var openRequest = new OpenPortRequest
            {
                DeviceId = devices.Content.AvailableCarts.First().DeviceId
            };
            var r = await f.Client.GetAsync<OpenPortEndpoint, OpenPortRequest, OpenPortResponse>(openRequest);

            // Assert
            r.Should().BeSuccessful<OpenPortResponse>()
                .WithStatusCode(HttpStatusCode.OK)
                .WithContentNotNull();

            r.Content.Should().NotBeNull();
            r.Content.ConnectedCart.Should().NotBeNull();
            r.Content.ConnectedCart.DeviceId.Should().NotBeNullOrEmpty();
            r.Content.ConnectedCart.DeviceId.Should().Be(openRequest.DeviceId);
            r.Content.ConnectedCart.ComPort.Should().NotBeNullOrEmpty();            

            r.Content.Message.Should().Contain("Connection successful!");
        }
    }
}