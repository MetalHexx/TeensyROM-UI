using System.Net.ServerSentEvents;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace TeensyRom.Api.Http
{
    public static class ChannelExtensions
    {
        public static async IAsyncEnumerable<SseItem<T>> WriteObservableToChannel<T>(
            this Channel<SseItem<T>> channel,
            IObservable<SseItem<T>> observable,
            [EnumeratorCancellation] CancellationToken ct)
        {
            var subscription = observable
                .Buffer(TimeSpan.FromMilliseconds(100), 100)
                .Where(buffer => buffer.Count > 0)          
                .Subscribe(buffer =>
                {
                    if (ct.IsCancellationRequested) return;

                    foreach (var item in buffer)
                    {
                        channel.Writer.TryWrite(item);
                    }
                });

            try
            {
                while (await channel.Reader.WaitToReadAsync(ct))
                {
                    while (channel.Reader.TryRead(out var item))
                    {
                        yield return item;
                    }
                }
            }
            finally
            {
                subscription?.Dispose();
                channel.Writer.TryComplete();
            }
        }
    }
}
