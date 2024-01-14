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

        public void Log(string message, string hExColor)
        {
            var sb = new StringBuilder();

            _logs.OnNext(message
                .SplitAtCarriageReturn()
                .Select(line => line.WithColor(hExColor))
                .Aggregate(sb, (acc, line) => acc.AppendWithLimit(line))
                .ToString()
                .DropLastNewLine());
        }

        public void Internal(string message) => Log(message, "#b39ddb"); //lavendar
        public void InternalSuccess(string message) => Log(message, "#86c691"); //green
        public void InternalError(string message) => Log(message, "#cc666c"); //soft red
        public void External(string message) => Log($"[TR]: {message}", "#7FDBD6"); //teensy blue
        public void ExternalSuccess(string message) => Log($"[TR]: {message}", "#86c691"); //green
        public void ExternalError(string message) => Log($"[TR]: {message}", "#cc666c"); //soft red
    }
}