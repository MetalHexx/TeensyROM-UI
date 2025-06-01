using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using TeensyRom.Core.Logging;

namespace TeensyRom.Api.Services
{
    public class LoggingService : ILoggingService
    {
        private Subject<string> _logs = new();
        public IObservable<string> Logs => _logs.AsObservable();

        public void External(string message, string? deviceId = null)
        {
            var logMessage = deviceId is not null ? $"({deviceId}) External: {message}" : message;

            _logs.OnNext(logMessage);
            Debug.WriteLine(logMessage);
        }

        public void ExternalError(string message, string? deviceId = null)
        {
            var logMessage = deviceId is not null ? $"({deviceId}) External Error: {message}" : message;

            _logs.OnNext(logMessage);
            Debug.WriteLine(logMessage);
        }

        public void ExternalSuccess(string message, string? deviceId = null)
        {
            var logMessage = deviceId is not null ? $"({deviceId}) External Success: {message}" : message;

            _logs.OnNext(logMessage);            
            Debug.WriteLine(logMessage);
        }

        public void Internal(string message, string? deviceId = null)
        {
            var logMessage = deviceId is not null ? $"({deviceId}) Internal: {message}" : message;

            _logs.OnNext(logMessage);
            Debug.WriteLine(logMessage);
        }

        public void InternalError(string message, string? deviceId = null)
        {
            var logMessage = deviceId is not null ? $"({deviceId}) Internal Error: {message}" : message;

            _logs.OnNext(logMessage);            
            Debug.WriteLine(logMessage);
        }

        public void InternalWarning(string message, string? deviceId = null)
        {
            var logMessage = deviceId is not null ? $"({deviceId}) Internal Warning: {message}" : message;

            _logs.OnNext(logMessage);
            Debug.WriteLine(logMessage);
        }

        public void InternalSuccess(string message, string? deviceId = null)
        {
            var logMessage = deviceId is not null ? $"({deviceId}) Internal Success: {message}" : message;

            _logs.OnNext(message);
            Debug.WriteLine(logMessage);
        }
    }
}
