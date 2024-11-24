using System.Reactive.Linq;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Common;
using System.Text;
using System.Linq;
using System;
using System.IO;
using System.Reflection;

namespace TeensyRom.Ui.Services
{
    internal interface IUiLoggingService : ILoggingService
    {
        bool Enabled { get; set; }
    }
    internal class UiLoggingService : LoggingService, IUiLoggingService
    {
        public bool Enabled { get; set; } = true;

        private static readonly object _logFileLock = new();
        private string _logPath => Path.Combine(Assembly.GetExecutingAssembly().GetPath(), LogConstants.LogPath);
        public override void Log(string message, string hExColor, bool newLine = true)
        {
            lock (_logFileLock)
            {
                File.AppendAllText(_logPath, message + Environment.NewLine);
            }

            var sb = new StringBuilder();

            _logs.OnNext(message
                .SplitAtCarriageReturn()
                .Select(line => line.WithColor(hExColor))
                .Aggregate(sb, (acc, line) => acc.AppendWithLimit(line))
                .ToString()
                .DropLastNewLine());
            base.Log(message, hExColor);
        }
    }
}