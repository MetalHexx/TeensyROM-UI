using Microsoft.AspNetCore.Http.HttpResults;
using RadEndpoints;
using System.Net.ServerSentEvents;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using TeensyRom.Core.Logging;

namespace TeensyRom.Api.Endpoints.GetLogs
{
    public class GetLogsEndpoint(ILoggingService loggingService) : RadEndpoint
    {
        public override void Configure()
        {
            RouteBuilder.MapGet("/logs", (CancellationToken c) => Handle(c));
        }

        public ServerSentEventsResult<LogDto> Handle(CancellationToken c)
        {
            var channel = Channel.CreateUnbounded<SseItem<LogDto>>();

            var subscription = loggingService.Logs
                .Buffer(TimeSpan.FromMilliseconds(500), 10)
                .Subscribe(logBatch =>    {
                    foreach (var log in logBatch)
                    {
                        channel.Writer.TryWrite(new SseItem<LogDto>(new LogDto { Message = log }, "log")
                        {
                            ReconnectionInterval = TimeSpan.FromMinutes(1)
                        });
                    }
                });
            

            c.Register(() =>
            {
                subscription.Dispose();
                channel.Writer.Complete();
            });

            //TODO: fix the exception that gets thrown when the client disconnects.

            async IAsyncEnumerable<SseItem<LogDto>> GetLogs([EnumeratorCancellation] CancellationToken ct)
            {
                while (await channel.Reader.WaitToReadAsync(ct))
                {
                    while (channel.Reader.TryRead(out var item))
                    {
                        yield return item;
                    }
                }
            }

            return TypedResults.ServerSentEvents(GetLogs(c));
        }
    }
}
