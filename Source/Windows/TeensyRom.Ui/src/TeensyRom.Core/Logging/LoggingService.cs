using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace TeensyRom.Core.Logging
{
    public class LoggingService : ILoggingService
    {
        public IObservable<string> Logs => _logs.AsObservable();
        protected Subject<string> _logs = new();

        public void Log(string message)
        {
            _logs.OnNext(message);
        }
    }
}