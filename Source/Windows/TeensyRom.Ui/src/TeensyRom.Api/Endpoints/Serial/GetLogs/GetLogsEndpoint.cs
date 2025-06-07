using Microsoft.AspNetCore.Http.HttpResults;
using RadEndpoints;
using System.Net.ServerSentEvents;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using TeensyRom.Api.Http;
using TeensyRom.Core.Logging;

namespace TeensyRom.Api.Endpoints.Serial.GetLogs
{
    public class GetLogsEndpoint(ILoggingService loggingService) : RadEndpoint
    {
        public override void Configure()
        {
            RouteBuilder
                .MapGet("/logs", (CancellationToken c) => Handle(c))
                .RequireRateLimiting(EndpointHelper.GetLogsRateLimiter)
                .ExcludeFromDescription();
        }

        public ServerSentEventsResult<LogDto> Handle(CancellationToken c)
        {
            var channel = Channel.CreateUnbounded<SseItem<LogDto>>();

            var subscription = loggingService.Logs
                .Buffer(TimeSpan.FromMilliseconds(500), 10)
                .Subscribe(logBatch =>
                {
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
                channel.Writer.TryComplete();
            });

            return TypedResults.ServerSentEvents(ReadLogItemsAsync(channel, c));
        }

        private async IAsyncEnumerable<SseItem<LogDto>> ReadLogItemsAsync(Channel<SseItem<LogDto>> channel, [EnumeratorCancellation] CancellationToken ct)
        {
            while (true)
            {
                if (ct.IsCancellationRequested)
                    yield break;

                var waitTask = channel.Reader.WaitToReadAsync(ct).AsTask();

                bool canRead = false;
                try
                {
                    canRead = await waitTask.ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    yield break;
                }

                if (!canRead)
                    yield break;

                while (channel.Reader.TryRead(out var item))
                {
                    yield return item;
                }
            }
        }
    }
}
