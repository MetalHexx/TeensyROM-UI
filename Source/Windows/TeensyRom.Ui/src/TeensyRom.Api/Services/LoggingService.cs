using System.Diagnostics;
using TeensyRom.Core.Logging;

namespace TeensyRom.Api.Services
{
    public class LoggingService : ILoggingService
    {
        public IObservable<string> Logs => throw new NotImplementedException();

        public void External(string message)
        {
            Debug.WriteLine($"External: {message}");
        }

        public void ExternalError(string message)
        {
            Debug.WriteLine($"External Error: {message}");
        }

        public void ExternalSuccess(string message)
        {
            Debug.WriteLine($"External Success: {message}");
        }

        public void Internal(string message)
        {
            Debug.WriteLine($"Internal: {message}");
        }

        public void InternalError(string message)
        {
            Debug.WriteLine($"Internal Error: {message}");
        }

        public void InternalSuccess(string message)
        {
            Debug.WriteLine($"Internal Success: {message}");
        }
    }
}
