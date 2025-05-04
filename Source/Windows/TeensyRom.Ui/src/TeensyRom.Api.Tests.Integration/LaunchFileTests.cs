using FluentAssertions;
using TeensyRom.Api.Endpoints.Files.LaunchFile;
using TeensyRom.Api.Endpoints.OpenPort;

namespace TeensyRom.Api.Tests.Integration
{
    [Collection("Endpoint")]
    public class LaunchFileTests(EndpointFixture f)

    {
        [Fact]
        public async void When_LaunchCalled_WithValidPath_ReturnsSuccess()
        {
            // Arrange
            var request = new LaunchFileRequest
            {
                Path = "/music/MUSICIANS/L/LukHash/Alpha.sid"
            };
            await f.Client.PostAsync<OpenPortEndpoint, OpenPortResponse>();

            // Act
            var r = await f.Client.GetAsync<LaunchFileEndpoint, LaunchFileRequest, LaunchFileResponse>(request);

            // Assert
            r.Should().BeSuccessful<LaunchFileResponse>()
                .WithStatusCode(HttpStatusCode.OK)
                .WithContentNotNull();
                        
            r.Content.Message.Should().Contain("Success");
        }
    }
}