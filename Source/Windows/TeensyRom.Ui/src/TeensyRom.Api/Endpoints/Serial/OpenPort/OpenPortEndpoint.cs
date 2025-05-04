using RadEndpoints;
using TeensyRom.Core.Serial.State;

namespace TeensyRom.Api.Endpoints.OpenPort
{
    public class OpenPortEndpoint (ISerialStateContext serial) : RadEndpointWithoutRequest<OpenPortResponse>
    {
        public override void Configure()
        {
            Post("open")
                .Produces<OpenPortResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithDocument(tag: "Serial", desc: "Automatically connects to the first available TeensyRom cartridge.");
        }

        public override Task Handle(CancellationToken _)
        {
            var comPort = serial.OpenPort();
            Response = new();

            if (comPort is not null) 
            {
                Response.ComPort = comPort;
                Response.Message = $"Successfully connected to {comPort}";
                Send();
                return Task.CompletedTask;
            }
            Response.Message = "No TeensyRom cartridge found.  Retrying...";
            SendNotFound("No TeensyRom cartridge found.  A connection will continue to be attempted.  Make sure your Commodore is turned on and connected via serial.");
            return Task.CompletedTask;
        }
    }
}