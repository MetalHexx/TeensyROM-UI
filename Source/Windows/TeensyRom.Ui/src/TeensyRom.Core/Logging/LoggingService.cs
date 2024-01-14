using System.Drawing;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using TeensyRom.Core.Common;

namespace TeensyRom.Core.Logging
{
    public class LoggingService : ILoggingService
    {
        public IObservable<string> Logs => _logs.AsObservable();
        protected Subject<string> _logs = new();

        public void Log(string message)
        {
            var sb = new StringBuilder();

            _logs.OnNext(message
                .SplitAtNewlines()
                .Aggregate(sb, (acc, line) => acc.AppendWithLimit(line))
                .ToString()
                .DropLastNewLine());
        }

        public void Log(string message, Color color)
        {
            var sb = new StringBuilder();

            _logs.OnNext(message
                .SplitAtNewlines()
                .Select(line => line.WithColor(color))
                .Aggregate(sb, (acc, line) => acc.AppendWithLimit(line))
                .ToString()
                .DropLastNewLine());
        }

        public void Internal(string message) => Log(message, Color.Yellow);
        public void InternalSuccess(string message) => Log(message, Color.Magenta);
        public void InternalError(string message) => Log(message, Color.Cyan);
        public void External(string message) => Log($"TR: {message}", Color.SkyBlue);
        public void ExternalSuccess(string message) => Log($"TR: {message}", Color.Green);
        public void ExternalError(string message) => Log($"TR: {message}", Color.Red);
    }
}