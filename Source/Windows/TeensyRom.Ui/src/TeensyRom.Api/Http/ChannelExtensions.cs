////using System.Net.ServerSentEvents;
//using System.Reactive.Linq;
//using System.Reactive.Subjects;
//using System.Runtime.CompilerServices;
//using System.Threading.Channels;

//namespace TeensyRom.Api.Http
//{
//    public static class ChannelExtensions
//    {
//        public static async IAsyncEnumerable<SseItem<T>> WriteObservableToChannel<T>(
//            this Channel<SseItem<T>> channel,
//            IObservable<SseItem<T>> observable,
//            [EnumeratorCancellation] CancellationToken ct)
//        {
//            var subscription = observable
//                .Subscribe(item =>
//                {
//                    if (ct.IsCancellationRequested) return;

//                    channel.Writer.TryWrite(item);
//                },
//                onError: ex => channel.Writer.TryComplete(ex),
//                onCompleted: () => channel.Writer.TryComplete());

//            try
//            {
//                while (!ct.IsCancellationRequested && await channel.Reader.WaitToReadAsync(ct).ConfigureAwait(false))
//                {
//                    while (channel.Reader.TryRead(out var item))
//                    {
//                        if (!EqualityComparer<SseItem<T>>.Default.Equals(item, default))
//                        {
//                            yield return item;
//                        }
//                    }
//                }
//            }
//            finally
//            {
//                subscription?.Dispose();
//                channel.Writer.TryComplete();
//            }
//        }
//    }
//}
