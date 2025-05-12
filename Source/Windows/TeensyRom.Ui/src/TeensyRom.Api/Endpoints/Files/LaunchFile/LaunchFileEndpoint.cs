using MediatR;
using TeensyRom.Core.Commands.File.LaunchFile;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Api.Endpoints.Files.LaunchFile
{
    public class LaunchFileEndpoint(IMediator mediator) : RadEndpoint<LaunchFileRequest, LaunchFileResponse>
    {
        public override void Configure()
        {
            Get("device/{deviceId}/files/launch")
                .Produces<LaunchFileResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithDocument(tag: "Files", desc: "Launches a file given a valid path to a file stored on the TeensyRom.");
        }

        public override async Task Handle(LaunchFileRequest r, CancellationToken ct)
        {
            var testItem = new GameItem
            {
                Path = r.Path,
                Name = r.Path.GetFileNameFromPath(),
                Size = 575001
            };
            var launchCommand = new LaunchFileCommand(TeensyStorageType.SD, testItem, r.DeviceId);
            var result = await mediator.Send(launchCommand, ct);

            Response = new();

            if (!result.IsSuccess)
            {
                SendExternalError(result.Error);
                return;
            }
            Send();
        }
    }
}