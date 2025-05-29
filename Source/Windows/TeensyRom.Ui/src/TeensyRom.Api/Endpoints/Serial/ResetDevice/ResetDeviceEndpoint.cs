using MediatR;
using RadEndpoints;
using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Commands;

namespace TeensyRom.Api.Endpoints.ResetDevice
{
    public class ResetDeviceEndpoint(IDeviceConnectionManager deviceManager, IMediator mediator) : RadEndpoint<ResetDeviceRequest, ResetDeviceResponse>
    {
        public override void Configure()
        {
            Put("/devices/{deviceId}/reset")
                .Produces<ResetDeviceResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithName("ResetDevice")
                .WithSummary("Reset Device")
                .WithTags("Devices")
                .WithDescription(
                    "Resets a TeensyROM device.\n\n" +
                    "- Works the same as clicking the cartridge reset button."
                );
        }

        public override async Task Handle(ResetDeviceRequest r, CancellationToken ct)
        {
            var device = deviceManager.GetConnectedDevice(r.DeviceId);
            
            if (device is null)
            {
                SendNotFound($"The device {r.DeviceId} was not found.");
                return;
            }
            await mediator.Send(new ResetCommand
            {
                DeviceId = device.Cart.DeviceId
            });
            Response = new();

            Send();
        }
    }
}