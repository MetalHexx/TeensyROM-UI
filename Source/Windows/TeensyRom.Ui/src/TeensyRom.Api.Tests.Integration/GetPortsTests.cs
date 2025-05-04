using FluentAssertions;
using TeensyRom.Api.Endpoints.Serial.GetPorts;

namespace TeensyRom.Api.Tests.Integration
{
    [Collection("Endpoint")]
    public class GetPortsTests(EndpointFixture f)

    {
        [Fact]
        public async void When_Called_PortsReturned()
        {
            // Act
            var r = await f.Client.GetAsync<GetPortsEndpoint, GetPortsResponse>();

            // Assert
            r.Should().BeSuccessful<GetPortsResponse>()
                .WithStatusCode(HttpStatusCode.OK)
                .WithContentNotNull();

            r.Content.Message.Should().Be("Ports found");
            r.Content.Ports.Should().NotBeNullOrEmpty();
        }
    }
}