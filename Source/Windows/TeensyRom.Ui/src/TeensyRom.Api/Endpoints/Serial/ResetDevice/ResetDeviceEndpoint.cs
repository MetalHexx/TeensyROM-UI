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
            Put("/serial/{deviceId}/reset")
                .Produces<ResetDeviceResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithDocument(tag: "Serial", desc: "Resets the given device matching the give device id.");
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