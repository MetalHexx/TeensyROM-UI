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
                .Select(log => new LogDto { Message = log })
                .Select(logDto => new SseItem<LogDto>(logDto, "log")
                {
                    ReconnectionInterval = TimeSpan.FromMinutes(1)
                });

            var logStream = channel.WriteObservableToChannel(logsObservable, c);

            return TypedResults.ServerSentEvents(logStream);
        }
    }
}
