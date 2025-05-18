//using System.Reactive.Linq;
//using TeensyRom.Core.Abstractions;
//using TeensyRom.Core.Serial.State;

//namespace TeensyRom.Api.Endpoints.Serial.GetPorts
//{
//    public class GetPortsEndpoint(ISerialStateContext serialState) : RadEndpointWithoutRequest<GetPortsResponse>
//    {
//        public override void Configure()
//        {
//            Get("/devices/ports")
//                .WithDocument("Serial", "Get available serial ports.")
//                .Produces<GetPortsResponse>(StatusCodes.Status200OK);

//        }

//        public override async Task Handle(CancellationToken _)
//        {
//            var ports = await serialState.Ports.FirstAsync();

//            Response = new()
//            {
//                Message = ports.Length > 0 ? "Ports found" : "No ports found",
//                Ports = ports
//            };
//            Send();
//        }
//    }
//}
