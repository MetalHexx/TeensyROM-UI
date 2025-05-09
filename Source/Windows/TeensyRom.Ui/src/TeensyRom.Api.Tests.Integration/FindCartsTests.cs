using FluentAssertions;
using TeensyRom.Api.Endpoints.FindCarts;
using TeensyRom.Api.Endpoints.OpenPort;

namespace TeensyRom.Api.Tests.Integration
{
    [Collection("Endpoint")]
    public class FindCartsTests(EndpointFixture f)

    {
        [Fact]
        public async void When_Called_AvailableCartsReturned()
        {
            // Act
            var r = await f.Client.GetAsync<FindCartsEndpoint, FindCartsResponse>();

            // Assert
            r.Should()
                .BeSuccessful<FindCartsResponse>()
                .WithStatusCode(HttpStatusCode.OK)
                .WithContentNotNull();

            r.Content.Message.Should().Be("Success!");
            r.Content.AvailableCarts.Should().NotBeNullOrEmpty();            
            r.Content.ConnectedCarts.Should().BeEmpty();
        }

        [Fact]
        public async void Given_CartWasOpened_When_FindCalled_ConnectedCartsReturned()
        {
            // Arrange
            var initialCarts = await f.Client.GetAsync<FindCartsEndpoint, FindCartsResponse>();
            var expectedConnectedCart = initialCarts.Content.AvailableCarts.First();
            var expectedAvailableCount = initialCarts.Content.AvailableCarts.Count;
            var openRequest = new OpenPortRequest
            {
                DeviceId = expectedConnectedCart.DeviceId
            };
            var openResponse = await f.Client.GetAsync<OpenPortEndpoint, OpenPortRequest, OpenPortResponse>(openRequest);

            // Act
            var r = await f.Client.GetAsync<FindCartsEndpoint, FindCartsResponse>();

            // Assert
            r.Should().BeSuccessful<FindCartsResponse>()
                .WithStatusCode(HttpStatusCode.OK)
                .WithContentNotNull();

            r.Content.AvailableCarts.Should().NotBeNullOrEmpty();
            r.Content.AvailableCarts.Count.Should().Be(expectedAvailableCount);
            r.Content.ConnectedCarts.Count.Should().Be(1);
            r.Content.ConnectedCarts.First().DeviceId.Should().Be(expectedConnectedCart.DeviceId);
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