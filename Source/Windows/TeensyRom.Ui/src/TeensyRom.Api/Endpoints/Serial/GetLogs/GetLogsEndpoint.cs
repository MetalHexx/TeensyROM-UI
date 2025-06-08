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
                .ExcludeFromDescription();
        }

        public ServerSentEventsResult<LogDto> Handle(CancellationToken c)
        {
            var channel = Channel.CreateUnbounded<SseItem<LogDto>>();

            var logsObservable = loggingService.Logs
                .Where(log => !string.IsNullOrWhiteSpace(log))
                .Select(log => new LogDto { Message = log });

            var logSubscription = WriteLogsToChannel(logsObservable, channel, c);
            var logs = channel.WriteObservableToChannel(logSubscription, c);

            return TypedResults.ServerSentEvents(logs);
        }

        private IDisposable WriteLogsToChannel(IObservable<LogDto> logsObservable, Channel<SseItem<LogDto>> channel, CancellationToken ct)
        {
            return logsObservable
                .Subscribe(log =>
                {
                    if (ct.IsCancellationRequested)
                        return;
                    channel.Writer.TryWrite(new SseItem<LogDto>(log, "log")
                    {
                        ReconnectionInterval = TimeSpan.FromMinutes(1)
                    });
                });
        }
    }
}
