using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace TeensyRom.Api.Http
{
    public static class ChannelExtensions
    {
        public static async IAsyncEnumerable<SseItem<T>> WriteObservableToChannel<T>(
            this Channel<SseItem<T>> channel,            
            IDisposable observableSubscription,
            [EnumeratorCancellation] CancellationToken ct)
        {
            ct.Register(() =>
            {
                observableSubscription.Dispose();
                channel.Writer.TryComplete();
            });

            try
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
            finally
            {
                observableSubscription.Dispose();
                channel.Writer.TryComplete();
            }
        }

    }
}
