using RadEndpoints;
using System.Reactive.Linq;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.State;

namespace TeensyRom.Api.Endpoints.FindCarts
{
    public class FindCartsEndpoint(ISerialStateContext serialContext, ICartFinder cartFinder) : RadEndpointWithoutRequest<FindCartsResponse>
    {
        public override void Configure()
        {
            Get("/serial/carts")
                .Produces<FindCartsResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithDocument(tag: "Serial", desc: "Scans all the serial ports and attempts to identify ports that have a TeensyRom device.");
        }

        public override Task Handle(CancellationToken ct)
        {
            var serialState = serialContext.CurrentState.FirstOrDefaultAsync();
            var shouldReconnect = false;

            if (serialState is SerialBusyState or SerialConnectedState or SerialConnectionLostState)
            {
                serialContext.ClosePort();
            }
            var carts = cartFinder.DiscoverCarts();

            if(carts.Count == 0)
            {
                SendNotFound("No TeensyRom devices found.");
                return Task.CompletedTask;
            }
            Response = new()
            {
                Carts = carts,
                Message = "Success!"
            };
            if(shouldReconnect)
            {
                serialContext.OpenPort();
            }
            Send();
            return Task.CompletedTask;
        }
    }
}