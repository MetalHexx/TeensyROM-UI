using Microsoft.AspNetCore.Http.HttpResults;
using System.Net.ServerSentEvents;
using System.Threading.Channels;
using TeensyRom.Api.Services;

namespace TeensyRom.Api.Endpoints.Serial.GetLogs
{
    public class GetLogsEndpoint(IQueuedChannelLogger loggingService) : RadEndpoint
    {
        public override void Configure()
        {
            RouteBuilder
                .MapGet("/logs", (CancellationToken c) => Handle(c))
                .ExcludeFromDescription();
        }

        public ServerSentEventsResult<LogDto> Handle(CancellationToken c)
        {
            var sseChannel = Channel.CreateUnbounded<SseItem<LogDto>>();

            loggingService.StartChannelLogging();

            _ = ProcessLogsInBackground(loggingService.LogChannel, sseChannel.Writer, c);
            
            return TypedResults.ServerSentEvents(sseChannel.Reader.ReadAllAsync(c));
        }

        private static async Task ProcessLogsInBackground(
            ChannelReader<string> logReader,
            ChannelWriter<SseItem<LogDto>> writer,
            CancellationToken ct)
        {
            try
            {
                await foreach (var message in logReader.ReadAllAsync(ct))
                {
                    if (message != null)
                    {
                        var sseItem = new SseItem<LogDto>(
                            new LogDto { Message = message },
                            "log")
                        {
                            ReconnectionInterval = TimeSpan.FromMinutes(1)
                        };
                        
                        await writer.WriteAsync(sseItem, ct);
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                writer.TryComplete(ex);
            }
            finally
            {
                writer.TryComplete();
            }
        }
    }
}
