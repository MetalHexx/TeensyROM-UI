using System.Threading.Channels;
using TeensyRom.Core.Logging;

namespace TeensyRom.Api.Services
{
    public interface IQueuedChannelLogger : ILoggingService
    {
        void StartLogStream();
        void StopLogStream();
        Task StartLogStreaming(CancellationToken c);
    }
}