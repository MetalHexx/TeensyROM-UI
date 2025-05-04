using FluentAssertions;
using TeensyRom.Api.Endpoints.OpenPort;

namespace TeensyRom.Api.Tests.Integration
{
    [Collection("Endpoint")]
    public class OpenPortTests(EndpointFixture f)

    {
        [Fact]
        public async void When_Called_ResponseSuccessful()
        {
            // Act
            var r = await f.Client.PostAsync<OpenPortEndpoint, OpenPortResponse>();

            // Assert
            r.Should().BeSuccessful<OpenPortResponse>()
                .WithStatusCode(HttpStatusCode.OK)
                .WithContentNotNull();

            r.Content.ComPort.Should().NotBeNullOrEmpty();
            r.Content.Message.Should().Contain("Successfully connected");
        }
    }
}