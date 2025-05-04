using MediatR;
using RadEndpoints;
using TeensyRom.Core.Commands.File.LaunchFile;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Api.Endpoints.LaunchFile
{
    public class LaunchFileEndpoint(IMediator mediator) : RadEndpoint<LaunchFileRequest, LaunchFileResponse>
    {
        public override void Configure()
        {
            Get("/launch/{path}")
                .Produces<LaunchFileResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithDocument(tag: "Serial", desc: "Launches a file given a valid path to a file stored on the TeensyRom.");
        }

        public override async Task Handle(LaunchFileRequest r, CancellationToken ct)
        {
            var testItem = new SongItem
            {
                Path = r.Path.DecodeWebEncodedPath(),
                Name = r.Path.GetFileNameFromPath(),
                Size = 200 
            };
            var launchCommand = new LaunchFileCommand(TeensyStorageType.SD, testItem);
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