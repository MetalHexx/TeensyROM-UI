using System.Threading.Channels;
using TeensyRom.Core.Logging;

namespace TeensyRom.Api.Services
{
    public interface IQueuedChannelLogger : ILoggingService
    {
        ChannelReader<string> LogChannel { get; }
        void StartChannelLogging();
        void StopChannelQueue();
    }
}