using Microsoft.AspNetCore.Http.HttpResults;
using System.Diagnostics;
//using System.Net.ServerSentEvents;
using System.Threading.Channels;
using TeensyRom.Api.Services;

namespace TeensyRom.Api.Endpoints.Serial.GetLogs
{
    public class GetLogsEndpoint(IQueuedChannelLogger logService) : RadEndpoint
    {
        public override void Configure()
        {
            RouteBuilder
                .MapGet("/logs", (CancellationToken c) => Handle(c))
                .ExcludeFromDescription();
        }

        public IResult Handle(CancellationToken c)
        {
            // var sseChannel = Channel.CreateUnbounded<SseItem<LogDto>>();

            //_ = ProcessLogsInBackground(sseChannel.Writer, c);

            //return TypedResults.ServerSentEvents(sseChannel.Reader.ReadAllAsync(c));

            return TypedResults.Ok("Success");
        }

        //private async Task ProcessLogsInBackground(
        //    ChannelWriter<SseItem<LogDto>> writer,
        //    CancellationToken ct)
        //{
        //    logService.StartChannelLogging();
        //    try
        //    {               
        //        while (!ct.IsCancellationRequested)
        //        {
        //            var batch = logService.GetLogBatch(20)
        //                .Select(message => new SseItem<LogDto>(
        //                    new LogDto 
        //                    { 
        //                        Message = message 
        //                    },
        //                    "log"
        //                )
        //                {
        //                    ReconnectionInterval = TimeSpan.FromSeconds(3)
        //                })
        //                .ToList();


        //            foreach (var sseItem in batch)
        //            {
        //                await writer.WriteAsync(sseItem, ct);
        //            }
        //            await Task.Delay(1, ct);
        //        }
        //    }
        //    catch (OperationCanceledException)
        //    {
        //        // Expected when cancellation is requested
        //    }
        //    catch (Exception ex)
        //    {  
        //        Debug.WriteLine($"Error generating fake logs: {ex.Message}");
        //        writer.TryComplete(ex);
        //    }
        //    finally
        //    {
        //        writer.TryComplete();
        //       //logService.StopChannelQueue();
        //    }
        //}
    }
}
