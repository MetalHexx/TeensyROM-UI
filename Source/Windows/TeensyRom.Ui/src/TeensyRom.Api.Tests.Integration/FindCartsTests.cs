using FluentAssertions;
using TeensyRom.Api.Endpoints.FindCarts;

namespace TeensyRom.Api.Tests.Integration
{
    [Collection("Endpoint")]
    public class FindCartsTests(EndpointFixture f)

    {
        [Fact]
        public async void When_TeensyRomsActive_CartsReturned()
        {
            // Act
            var r = await f.Client.GetAsync<FindCartsEndpoint, FindCartsResponse>();

            // Assert
            r.Should().BeSuccessful<FindCartsResponse>()
                .WithStatusCode(HttpStatusCode.OK)
                .WithContentNotNull();

            r.Content.Message.Should().Be("Success!");
            r.Content.Carts.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async void When_TeensyRomsDeactivated_NotFoundReturned()
        {
            // Act
            var r = await f.Client.GetAsync<FindCartsEndpoint, ProblemDetails>();

            // Assert
            r.Should().BeProblem()
                .WithStatusCode(HttpStatusCode.NotFound);

            r.Content.Title.Should().Be("No TeensyRom devices found.");
        }
    }
}