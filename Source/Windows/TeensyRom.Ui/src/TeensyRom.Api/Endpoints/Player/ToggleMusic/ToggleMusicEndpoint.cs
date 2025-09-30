using MediatR;
using RadEndpoints;
using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Serial.Commands.ToggleMusic;

namespace TeensyRom.Api.Endpoints.Player.ToggleMusic
{
    public class ToggleMusicEndpoint(IMediator mediator, IDeviceConnectionManager deviceManager) : RadEndpoint<ToggleMusicRequest, ToggleMusicResponse>
    {
        public override void Configure()
        {
            Post("/devices/{deviceId}/toggle-music")
                .Produces<ToggleMusicResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status502BadGateway)
                .WithName("ToggleMusic")
                .WithSummary("Toggle Music Playback")
                .WithDescription("Toggles the play/pause state of the currently playing music on the TeensyRom device.")
                .WithTags("Player");
        }

        public override async Task Handle(ToggleMusicRequest r, CancellationToken ct)
        {
            var device = deviceManager.GetConnectedDevice(r.DeviceId);

            if (device is null)
            {
                SendNotFound($"The device {r.DeviceId} was not found.");
                return;
            }

            var toggleCommand = new ToggleMusicCommand(r.DeviceId);
            var result = await mediator.Send(toggleCommand, ct);

            if (!result.IsSuccess)
            {
                SendExternalError(result.Error);
                return;
            }

            Response = new()
            {
                Message = "Music toggle successful"
            };

            Send();
        }
    }
}